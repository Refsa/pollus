namespace Pollus.Engine.Serialization;

using Pollus.Core.Serialization;
using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Utils;

public class WorldSnapshot
{
    public required byte[] RawData { get; set; }
}

public struct SnapshotSerializeEvent
{
    public string Path { get; set; }
}

public struct SnapshotSerializeResultEvent
{
    public Handle<WorldSnapshot> SnapshotHandle { get; set; }
}

public struct SnapshotDeserializeEvent
{
    public Handle<WorldSnapshot> Snapshot { get; set; }
}

[SystemSet]
public partial class SnapshotSerializationPlugin : IPlugin
{
    [System(nameof(Serialize))] static readonly SystemBuilderDescriptor SerializeDescriptor = new()
    {
        Stage = CoreStage.Last,
        RunCriteria = new EventRunCriteria<SnapshotSerializeEvent>(),
        Dependencies = [typeof(ExclusiveSystemMarker)],
    };

    [System(nameof(Deserialize))] static readonly SystemBuilderDescriptor DeserializeDescriptor = new()
    {
        Stage = CoreStage.Last,
        RunCriteria = new EventRunCriteria<SnapshotDeserializeEvent>(),
        Dependencies = [typeof(ExclusiveSystemMarker)],
    };

    public void Apply(World world)
    {
        world.Events.InitEvent<SnapshotSerializeEvent>();
        world.Events.InitEvent<SnapshotDeserializeEvent>();
        world.Events.InitEvent<SnapshotSerializeResultEvent>();

        world.Schedule.AddSystemSet<SnapshotSerializationPlugin>();
    }

    static void Serialize(ISerialization serialization,
        EventReader<SnapshotSerializeEvent> reader, EventWriter<SnapshotSerializeResultEvent> writer,
        World world, AssetServer assetServer)
    {
        var events = reader.Read();
        var latest = events[^1];

        using var ser = serialization.Writer;
        ser.Writer.Clear();

        // Component Infos
        ser.Writer.Write(Component.ComponentInfos.Count);
        foreach (var (cid, info) in Component.ComponentInfos)
        {
            ser.Writer.Write(cid);
            ser.Writer.Write(info.SizeInBytes);
            ser.Writer.Write(info.Read);
            ser.Writer.Write(info.Write);
            ser.Writer.Write(info.TypeName);
        }

        // Resources

        // Archetypes and Component data
        var archetypes = world.Store.Archetypes.Where(a => FilterHelpers.RunArchetypeFilters(a, [All<SerializeTag>.Instance]));
        ser.Writer.Write(archetypes.Count());
        foreach (var archetype in archetypes)
        {
            var chunkInfo = archetype.GetChunkInfo();
            ser.Writer.Write(chunkInfo.ComponentIDs);
            ser.Writer.Write(archetype.EntityCount);

            var chunks = archetype.Chunks;
            ser.Writer.Write(chunks.Length);
            foreach (var chunk in chunks)
            {
                ser.Writer.Write(chunk.GetEntities());
                foreach (var cid in chunkInfo.ComponentIDs)
                {
                    ser.Writer.Write(chunk.GetComponentsNative(cid));
                }
            }
        }

        var handle = assetServer.Assets.Add(new WorldSnapshot { RawData = ser.Writer.Buffer.ToArray() }, latest.Path);
        writer.Write(new SnapshotSerializeResultEvent { SnapshotHandle = handle });
    }

    static void Deserialize(ISerialization serialization, EventReader<SnapshotDeserializeEvent> reader, Assets<WorldSnapshot> snapshots, World world)
    {
        var events = reader.Read();
        var latest = events[^1];
        var snapshotData = snapshots.Get(latest.Snapshot);
        if (snapshotData is null) return;

        using var deser = serialization.Reader;
        deser.Reader.Init(snapshotData.RawData);

        var componentLookup = new Dictionary<ComponentID, Component.Info>();

        // Component Infos
        var componentCount = deser.Reader.Read<int>();
        for (int i = 0; i < componentCount; i++)
        {
            var cid = deser.Reader.Read<ComponentID>();
            var sizeInBytes = deser.Reader.Read<int>();
            var read = deser.Reader.Read<bool>();
            var write = deser.Reader.Read<bool>();
            var typeName = deser.Reader.ReadString();

            var type = Type.GetType(typeName) ?? throw new InvalidOperationException($"Type {typeName} not found");

            var info = Component.Register(new Component.Info()
            {
                ID = -1,
                SizeInBytes = sizeInBytes,
                Type = type,
                TypeName = typeName,
                Read = read,
                Write = write,
            });
            componentLookup[cid] = info;
        }

        // Resources

        // Archetypes and Component data
        Span<NativeArray<byte>> componentMemory = stackalloc NativeArray<byte>[componentLookup.Count];
        var archetypeCount = deser.Reader.Read<int>();
        for (int i = 0; i < archetypeCount; i++)
        {
            var componentIDs = deser.Reader.ReadArray<ComponentID>().Select(e => componentLookup[e].ID).ToArray();
            var entityCount = deser.Reader.Read<int>();
            var archetypeInfo = world.Store.GetOrCreateArchetype(ArchetypeID.Create(componentIDs), componentIDs);
            archetypeInfo.Archetype.Preallocate(entityCount);

            var chunkCount = deser.Reader.Read<int>();
            for (int j = 0; j < chunkCount; j++)
            {
                var chunkIndex = -1;
                var chunkEntityCount = deser.Reader.Read<int>();
                for (int k = 0; k < chunkEntityCount; k++)
                {
                    var entity = deser.Reader.Read<Entity>();
                    (chunkIndex, var rowIndex) = archetypeInfo.Archetype.AddEntity(entity);
                    world.Store.Entities.Append(new Entities.EntityInfo
                    {
                        Entity = entity,
                        IsAlive = true,
                        ArchetypeIndex = i,
                        ChunkIndex = chunkIndex,
                        RowIndex = rowIndex
                    });
                }

                Guard.IsTrue(chunkIndex != -1, "Chunk index is -1");
                ref var chunk = ref archetypeInfo.Archetype.GetChunk(chunkIndex);
                for (int k = 0; k < componentIDs.Length; k++)
                {
                    var componentData = deser.Reader.ReadArray<byte>();
                    componentData.CopyTo(chunk.GetComponentsNative(componentIDs[k]));
                }
            }
        }

        world.Store.Entities.Recalcuate();
    }
}
