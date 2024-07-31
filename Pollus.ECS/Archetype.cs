namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public record struct ArchetypeID(int Hash)
{
    public static ArchetypeID Create(int hash)
    {
        return new ArchetypeID(hash);
    }

    public static ArchetypeID Create(Span<ComponentID> cids)
    {
        var hash = 0;
        for (int i = 0; i < cids.Length; i++)
        {
            hash = HashCode.Combine(hash, cids[i]);
        }
        return new ArchetypeID(hash);
    }

    public static implicit operator int(ArchetypeID id) => id.Hash;
    public static implicit operator ArchetypeID(int hash) => new(hash);
}

public partial class Archetype : IDisposable
{
    const uint MAX_CHUNK_SIZE = 1u << 16;

    public record struct ChunkInfo
    {
        public int RowsPerChunk { get; init; }
        public ComponentID[] ComponentIDs { get; init; }
    }

    public readonly record struct EntityInfo
    {
        public Entity Entity { get; init; }
        public int ChunkIndex { get; init; }
        public int RowIndex { get; init; }
    }

    readonly ArchetypeID id;
    readonly ChunkInfo chunkInfo;

    NativeArray<ArchetypeChunk> chunks;
    int entityCount;

    public ArchetypeID ID => id;
    public Span<ArchetypeChunk> Chunks => chunks.AsSpan();
    public int EntityCount => entityCount;
    public ChunkInfo GetChunkInfo() => chunkInfo;

    public Archetype(Span<ComponentID> cids) : this(ArchetypeID.Create(cids), cids) { }

    public Archetype(ArchetypeID aid, Span<ComponentID> cids)
    {
        id = aid;

        var rowStride = 0;
        foreach (var cid in cids)
        {
            var cinfo = Component.GetInfo(cid);
            rowStride += cinfo.SizeInBytes;
        }
        var rowsPerChunk = rowStride switch
        {
            > 0 => (MAX_CHUNK_SIZE - Unsafe.SizeOf<ArchetypeChunk>()) / (uint)rowStride,
            _ => 0,
        };

        chunkInfo = new ChunkInfo
        {
            RowsPerChunk = (int)rowsPerChunk,
            ComponentIDs = [.. cids],
        };
        chunks = new(0);
    }

    public void Dispose()
    {
        foreach (var chunk in chunks)
        {
            chunk.Dispose();
        }
        chunks.Dispose();
    }

    public EntityInfo AddEntity(in Entity entity)
    {
        entityCount++;

        ref var chunk = ref GetVacantChunk();
        var row = chunk.AddEntity(entity);
        return new()
        {
            ChunkIndex = chunks.Length - 1,
            RowIndex = row
        };
    }

    public EntityInfo? RemoveEntity(in EntityInfo info)
    {
        entityCount--;

        ref var chunk = ref chunks[info.ChunkIndex];
        ref var lastChunk = ref Unsafe.NullRef<ArchetypeChunk>();
        for (int i = chunks.Length - 1; i >= 0; i--)
        {
            if (chunks[i].Count > 0)
            {
                lastChunk = ref chunks[i];
                break;
            }
        }

        var movedEntity = chunk.SwapRemoveEntity(info.RowIndex, ref lastChunk);
        if (movedEntity != Entity.NULL)
        {
            var movedInfo = new EntityInfo
            {
                Entity = movedEntity,
                ChunkIndex = chunks.Length - 1,
                RowIndex = lastChunk.Count - 1
            };
            return movedInfo;
        }

        return null;
    }

    public void SetComponent<C>(int chunkIndex, int rowIndex, in C component) where C : unmanaged, IComponent
    {
        ref var chunk = ref chunks[chunkIndex];
        chunk.SetComponent(rowIndex, component);
    }

    public ref C GetComponent<C>(int chunkIndex, int rowIndex) where C : unmanaged, IComponent
    {
        ref var chunk = ref chunks[chunkIndex];
        return ref chunk.GetComponent<C>(rowIndex);
    }

    public ref ArchetypeChunk GetChunk(int chunkIndex)
    {
        return ref chunks[chunkIndex];
    }

    public void Optimize()
    {
        int newLength = chunks.Length;
        for (int i = chunks.Length - 1; i >= 0; i--)
        {
            if (chunks[i].Count == 0)
            {
                chunks[i].Dispose();
                newLength--;
            }
        }

        if (newLength != chunks.Length)
        {
            chunks.Resize(newLength);
        }
    }

    ref ArchetypeChunk GetVacantChunk()
    {
        if (chunks.Length == 0 || chunks[^1].Count >= chunkInfo.RowsPerChunk)
        {
            chunks.Resize(chunks.Length + 1);
            chunks[^1] = new(chunkInfo.ComponentIDs, chunkInfo.RowsPerChunk);
        }
        return ref chunks[^1];
    }
}
