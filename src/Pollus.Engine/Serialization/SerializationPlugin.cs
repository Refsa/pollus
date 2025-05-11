namespace Pollus.Engine.Serialization;

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Pollus.Collections;
using Pollus.Core.Serialization;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Utils;

public struct SerializeTag : IComponent
{

}

public struct SerializeEvent
{
    public string Path { get; set; }
}

public struct SerializeResultEvent
{
    public Handle<WorldSnapshot> SnapshotHandle { get; set; }
}

public struct DeserializeEvent
{
    public Handle<WorldSnapshot> Snapshot { get; set; }
}

public class WorldSnapshot
{
    public required byte[] RawData { get; set; }
}

[SystemSet]
public partial class SerializationPlugin<TSerializer, TDeserializer> : IPlugin
    where TSerializer : ISerializer, new()
    where TDeserializer : IDeserializer, new()
{
    [System(nameof(Serialize))]
    static readonly SystemBuilderDescriptor SerializeDescriptor = new()
    {
        Stage = CoreStage.Last,
        RunCriteria = new EventRunCriteria<SerializeEvent>(),
        Locals = [new Local<TSerializer>(new())],
    };

    [System(nameof(Deserialize))]
    static readonly SystemBuilderDescriptor DeserializeDescriptor = new()
    {
        Stage = CoreStage.Last,
        RunCriteria = new EventRunCriteria<DeserializeEvent>(),
    };

    public void Apply(World world)
    {
        world.Events.InitEvent<SerializeEvent>();
        world.Events.InitEvent<DeserializeEvent>();
        world.Events.InitEvent<SerializeResultEvent>();

        world.Schedule.AddSystemSet<SerializationPlugin<TSerializer, TDeserializer>>();
    }

    static void Serialize(Local<TSerializer> serializer,
        EventReader<SerializeEvent> reader, EventWriter<SerializeResultEvent> writer,
        World world, AssetServer assetServer)
    {
        var events = reader.Read();
        var latest = events[^1];

        var ser = serializer.Value;
        ser.Clear();

        // Component Infos
        ser.Write(Component.ComponentInfos.Count);
        foreach (var (cid, info) in Component.ComponentInfos)
        {
            ser.Write(cid);
            ser.Write(info.SizeInBytes);
            ser.Write(info.Read);
            ser.Write(info.Write);
            ser.Write(info.TypeName);
        }

        // Resources

        // Archetypes and Component data
        var archetypes = world.Store.Archetypes.Where(a => FilterHelpers.RunArchetypeFilters(a, [All<SerializeTag>.Instance])).ToList();
        ser.Write(archetypes.Count);
        foreach (var archetype in archetypes)
        {
            var chunkInfo = archetype.GetChunkInfo();
            ser.Write(chunkInfo.ComponentIDs);
            ser.Write(archetype.EntityCount);

            var chunks = archetype.Chunks;
            ser.Write(chunks.Length);
            foreach (var chunk in chunks)
            {
                ser.Write(chunk.GetEntities());
                foreach (var cid in chunkInfo.ComponentIDs)
                {
                    ser.Write(chunk.GetComponentsNative(cid));
                }
            }
        }

        var handle = assetServer.Assets.Add(new WorldSnapshot { RawData = ser.Buffer.ToArray() }, latest.Path);
        writer.Write(new SerializeResultEvent { SnapshotHandle = handle });
    }

    static void Deserialize(EventReader<DeserializeEvent> reader, Assets<WorldSnapshot> snapshots, World world)
    {
        var events = reader.Read();
        var latest = events[^1];
        var snapshotData = snapshots.Get(latest.Snapshot);
        if (snapshotData is null) return;

        var deser = new TDeserializer();
        deser.Init(snapshotData.RawData);

        var componentLookup = new Dictionary<ComponentID, Component.Info>();

        var componentCount = deser.Read<int>();
        for (int i = 0; i < componentCount; i++)
        {
            var cid = deser.Read<ComponentID>();
            var sizeInBytes = deser.Read<int>();
            var read = deser.Read<bool>();
            var write = deser.Read<bool>();
            var typeName = deser.ReadString();

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

        Span<NativeArray<byte>> componentMemory = stackalloc NativeArray<byte>[componentLookup.Count];
        var archetypeCount = deser.Read<int>();
        for (int i = 0; i < archetypeCount; i++)
        {
            var componentIDs = deser.ReadArray<ComponentID>().Select(e => componentLookup[e].ID).ToArray();
            var entityCount = deser.Read<int>();
            var archetypeInfo = world.Store.GetOrCreateArchetype(ArchetypeID.Create(componentIDs), componentIDs);
            archetypeInfo.Archetype.Preallocate(entityCount);

            var chunkCount = deser.Read<int>();
            for (int j = 0; j < chunkCount; j++)
            {
                var chunkIndex = -1;
                var chunkEntityCount = deser.Read<int>() / Unsafe.SizeOf<Entity>();
                for (int k = 0; k < chunkEntityCount; k++)
                {
                    var entity = deser.Read<Entity>();
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

                ref var chunk = ref archetypeInfo.Archetype.GetChunk(chunkIndex);
                for (int k = 0; k < componentCount; k++)
                {
                    var componentData = deser.ReadArray<byte>();
                    componentData.CopyTo(chunk.GetComponentsNative(componentIDs[k]));
                }
            }
        }

        world.Store.Entities.Recalcuate();
    }
}

public class BinarySerializer : ISerializer, IDisposable
{
    byte[] buffer;
    int cursor;

    public ReadOnlySpan<byte> Buffer => buffer.AsSpan(0, cursor);

    public BinarySerializer()
    {
        buffer = ArrayPool<byte>.Shared.Rent(1024);
        cursor = 0;
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }

    public void Clear()
    {
        cursor = 0;
    }

    void Resize<T>(int count)
        where T : unmanaged
    {
        var neededSize = cursor + (count * Unsafe.SizeOf<T>());
        if (neededSize < buffer.Length) return;

        var newSize = Math.Max(neededSize, buffer.Length * 2);
        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        buffer.CopyTo(newBuffer, 0);
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = newBuffer;
    }

    public void Write(ReadOnlySpan<byte> data)
    {
        Resize<byte>(data.Length + sizeof(int));
        Write(data.Length);
        data.CopyTo(buffer.AsSpan(cursor));
        cursor += data.Length;
    }

    public void Write<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        Write(MemoryMarshal.Cast<T, byte>(data));
    }

    public void Write<T>(T value) where T : unmanaged
    {
        Resize<T>(1);
        MemoryMarshal.Write(buffer.AsSpan(cursor), in value);
        cursor += Unsafe.SizeOf<T>();
    }

    public void Write<T>(T[] values) where T : unmanaged
    {
        Write(MemoryMarshal.Cast<T, byte>(values.AsSpan()));
    }

    public void Write(string value)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        Resize<byte>(byteCount + sizeof(int));
        Write(byteCount);
        Encoding.UTF8.GetBytes(value, buffer.AsSpan(cursor));
        cursor += byteCount;
    }
}

public struct BinaryDeserializer : IDeserializer
{
    byte[] buffer;
    int cursor;

    public void Init(byte[] data)
    {
        buffer = data;
        cursor = 0;
    }

    public ReadOnlySpan<T> ReadSpan<T>() where T : unmanaged
    {
        var bytes = Read<int>();
        var span = buffer.AsSpan(cursor, bytes);
        cursor += bytes;
        return MemoryMarshal.Cast<byte, T>(span);
    }

    public void ReadTo<T>(Span<T> target) where T : unmanaged
    {
        var span = ReadSpan<T>();
        span.CopyTo(target);
    }

    public T Read<T>() where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        var value = MemoryMarshal.Read<T>(buffer.AsSpan(cursor));
        cursor += size;
        return value;
    }

    public T[] ReadArray<T>() where T : unmanaged
    {
        return ReadSpan<T>().ToArray();
    }

    public string ReadString()
    {
        var length = Read<int>();
        var value = Encoding.UTF8.GetString(buffer.AsSpan(cursor, length));
        cursor += length;
        return value;
    }
}
