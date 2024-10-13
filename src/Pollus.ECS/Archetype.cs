namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Collections;

public record struct ArchetypeID(int Hash)
{
    public static ArchetypeID Create(int hash)
    {
        return new ArchetypeID(hash);
    }

    public static ArchetypeID Create(scoped in Span<ComponentID> cids)
    {
        var hash = 0;
        for (int i = 0; i < cids.Length; i++)
        {
            hash = HashCode.Combine(hash, cids[i].ID);
        }
        return new ArchetypeID(hash);
    }

    public static explicit operator int(ArchetypeID id) => id.Hash;
    public static explicit operator ArchetypeID(int hash) => new(hash);

    public ArchetypeID With<C>() where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        return new ArchetypeID(HashCode.Combine(Hash, cid));
    }

    public ArchetypeID With(in ComponentID cid)
    {
        return new ArchetypeID(HashCode.Combine(Hash, cid.ID));
    }

    public override int GetHashCode() => Hash;
}

public partial class Archetype : IDisposable
{
    const uint MAX_CHUNK_SIZE = 1u << 16;

    public record struct ChunkInfo
    {
        public int RowsPerChunk { get; init; }
        public ComponentID[] ComponentIDs { get; init; }
    }

    readonly ArchetypeID id;
    readonly ChunkInfo chunkInfo;

    ulong version = 0;

    NativeArray<ArchetypeChunk> chunks;
    int lastChunkIndex = -1;
    int entityCount;

    public ArchetypeID ID => id;
    public Span<ArchetypeChunk> Chunks => chunks.Slice(0, lastChunkIndex + 1);
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
            _ => 1,
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

    public (int ChunkIndex, int RowIndex) AddEntity(in Entity entity)
    {
        entityCount++;
        ref var chunk = ref GetVacantChunk();
        var row = chunk.AddEntity(entity);
        return (lastChunkIndex, row);
    }

    public Entity RemoveEntity(int chunkIndex, int rowIndex)
    {
        if (entityCount == 0 || lastChunkIndex == -1) return Entity.NULL;
        entityCount = int.Max(0, entityCount - 1);

        ref var chunk = ref chunks[chunkIndex];
        ref var lastChunk = ref chunks[lastChunkIndex];

        var movedEntity = Entity.NULL;
        if (chunkIndex == lastChunkIndex && rowIndex == chunk.Count - 1)
        {
            lastChunk.RemoveEntity(rowIndex);
        }
        else
        {
            movedEntity = chunk.SwapRemoveEntity(rowIndex, ref lastChunk);
        }

        if (lastChunk.Count == 0)
        {
            lastChunkIndex = int.Max(0, lastChunkIndex - 1);
        }

        return movedEntity;
    }

    public Entity MoveEntity(int srcChunkIndex, int srcRowIndex, Archetype destination, int dstChunkIndex, int dstRowIndex)
    {
        ref var srcChunk = ref chunks[srcChunkIndex];
        ref var dstChunk = ref destination.chunks[dstChunkIndex];

        for (int i = 0; i < chunkInfo.ComponentIDs.Length; i++)
        {
            var cid = chunkInfo.ComponentIDs[i];
            if (destination.HasComponent(cid) is false) continue;

            var comp = srcChunk.GetComponent(srcRowIndex, cid);
            dstChunk.SetComponent(dstRowIndex, cid, comp);
        }

        return RemoveEntity(srcChunkIndex, srcRowIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetComponent<C>(int chunkIndex, int rowIndex, scoped in C component) where C : unmanaged, IComponent
    {
        ref var chunk = ref chunks[chunkIndex];
        chunk.SetComponent(rowIndex, component);
        chunk.SetFlag<C>(rowIndex, ComponentFlags.Changed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ref C GetComponent<C>(int chunkIndex, int rowIndex) where C : unmanaged, IComponent
    {
        ref var chunk = ref chunks[chunkIndex];
        return ref chunk.GetComponent<C>(rowIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasComponent<C>()
            where C : unmanaged, IComponent
    {
        return chunkInfo.ComponentIDs.Contains(Component.GetInfo<C>().ID);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasComponent(in ComponentID cid)
    {
        return chunkInfo.ComponentIDs.Contains(cid);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasAll(scoped in ReadOnlySpan<ComponentID> componentIDs)
    {
        foreach (var cid in componentIDs)
        {
            if (HasComponent(cid) is false) return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasAny(scoped in ReadOnlySpan<ComponentID> componentIDs)
    {
        foreach (var cid in componentIDs)
        {
            if (HasComponent(cid)) return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ref ArchetypeChunk GetChunk(int chunkIndex)
    {
        return ref chunks[chunkIndex];
    }

    public void Preallocate(int count)
    {
        int prevLength = chunks.Length;
        int required = count / chunkInfo.RowsPerChunk + 1;
        chunks.Resize(int.Max(required, prevLength));
        for (int i = prevLength; i < chunks.Length; i++)
        {
            chunks[i] = new(chunkInfo.ComponentIDs, chunkInfo.RowsPerChunk);
            chunks[i].Tick(version);
        }

        lastChunkIndex = int.Max(0, lastChunkIndex);
    }

    public void Optimize()
    {
        if (lastChunkIndex == chunks.Length) return;
        for (int i = lastChunkIndex + 1; i < chunks.Length; i++)
        {
            chunks[i].Dispose();
        }

        chunks.Resize(lastChunkIndex + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    ref ArchetypeChunk GetVacantChunk()
    {
        if (lastChunkIndex == -1 || chunks[lastChunkIndex].Count >= chunkInfo.RowsPerChunk)
        {
            if (lastChunkIndex < chunks.Length - 1)
            {
                lastChunkIndex++;
                chunks[lastChunkIndex].Tick(version);
            }
            else
            {
                chunks.Resize(chunks.Length + 1);
                chunks[^1] = new(chunkInfo.ComponentIDs, chunkInfo.RowsPerChunk);
                chunks[^1].Tick(version);
                lastChunkIndex = chunks.Length - 1;
            }
        }
        return ref chunks[lastChunkIndex];
    }

    public void Tick(ulong version)
    {
        this.version = version;
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Tick(version);
        }
    }
}

public ref struct ArchetypeChunkEnumerable
{
    readonly List<Archetype> archetypes;
    readonly ReadOnlySpan<ComponentID> componentIDs;
    readonly FilterArchetypeDelegate? filterArchetype;
    readonly FilterChunkDelegate? filterChunk;

    public ArchetypeChunkEnumerable(
        List<Archetype> archetypes, scoped in ReadOnlySpan<ComponentID> componentIDs,
        FilterArchetypeDelegate? filterArchetype = null, FilterChunkDelegate? filterChunk = null)
    {
        this.archetypes = archetypes;
        this.filterArchetype = filterArchetype;
        this.filterChunk = filterChunk;
        this.componentIDs = componentIDs;
    }

    public ChunkEnumerator GetEnumerator() => new(in this);

    public ref struct Enumerator
    {
        readonly List<Archetype> archetypes;
        readonly ReadOnlySpan<ComponentID> componentIDs;
        readonly FilterArchetypeDelegate? filterArchetype;
        int index;

        public Enumerator(scoped in ArchetypeChunkEnumerable filter)
        {
            archetypes = filter.archetypes;
            componentIDs = filter.componentIDs;
            filterArchetype = filter.filterArchetype;
            index = -1;
        }

        public Archetype Current => archetypes[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool MoveNext()
        {
            while (++index < archetypes.Count)
            {
                if (archetypes[index].EntityCount == 0) continue;
                if (archetypes[index].HasAll(componentIDs) is false) continue;
                if (filterArchetype is null || filterArchetype(archetypes[index]))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public ref struct ChunkEnumerator
    {
        readonly FilterChunkDelegate? filterChunk;
        Enumerator enumerator;

        ref ArchetypeChunk current;
        ref ArchetypeChunk end;
        public ref ArchetypeChunk Current => ref current;

        public ChunkEnumerator(scoped in ArchetypeChunkEnumerable filter)
        {
            filterChunk = filter.filterChunk;
            enumerator = new(in filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool MoveNext()
        {
            if (Unsafe.IsNullRef(ref current) || !Unsafe.IsAddressLessThan(ref current, ref end))
            {
                if (enumerator.MoveNext() is false) return false;
                var archetype = enumerator.Current;
                current = ref archetype.Chunks[0];
                end = ref archetype.Chunks[^1];
            }
            else
            {
                current = ref Unsafe.Add(ref current, 1);
                if (current.Count == 0) return MoveNext();
            }

            if (filterChunk is not null && filterChunk(in current) is false)
            {
                return MoveNext();
            }

            return true;
        }
    }
}