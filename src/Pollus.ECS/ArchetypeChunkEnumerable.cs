namespace Pollus.ECS;

using System.Runtime.CompilerServices;

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