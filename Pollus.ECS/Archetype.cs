using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pollus.ECS;

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

public class Archetypes : IDisposable
{
    readonly Dictionary<ArchetypeID, Archetype> archetypes = [];

    public void Dispose()
    {
        foreach (var archetype in archetypes.Values)
        {
            archetype.Dispose();
        }
    }
}

public class Archetype : IDisposable
{
    const uint MAX_CHUNK_SIZE = 1u << 16;

    public record struct ChunkInfo
    {
        public int RowsPerChunk { get; init; }
        public ComponentID[] ComponentIDs { get; init; }
    }

    public record struct EntityInfo
    {
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
        var rowsPerChunk = MAX_CHUNK_SIZE / (uint)rowStride;

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

    public EntityInfo AddEntity()
    {
        entityCount++;

        ref var chunk = ref GetVacantChunk();
        var row = chunk.AddEntity();
        return new()
        {
            ChunkIndex = chunks.Length - 1,
            RowIndex = row
        };
    }

    public void RemoveEntity(in EntityInfo info)
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

        if (lastChunk.Count == 0)
        {
            chunk.RemoveEntity();
            return;
        }

        chunk.SwapMoveEntity(info.RowIndex, ref lastChunk);
        chunk.RemoveEntity();
    }

    public void SetComponent<C>(in EntityInfo info, in C component) where C : unmanaged, IComponent
    {
        ref var chunk = ref chunks[info.ChunkIndex];
        chunk.SetComponent(info.RowIndex, component);
    }

    public ref C GetComponent<C>(in EntityInfo info) where C : unmanaged, IComponent
    {
        ref var chunk = ref chunks[info.ChunkIndex];
        return ref chunk.GetComponent<C>(info.RowIndex);
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
