namespace Pollus.ECS;

using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

public struct Query<C0> : IQuery, IQueryCreate<Query<C0>>
    where C0 : unmanaged, IComponent
{
    public class Filter<TFilters> : IQuery, IQueryCreate<Filter<TFilters>>
        where TFilters : ITuple, new()
    {
        public static Component.Info[] Infos => infos;
        public static Filter<TFilters> Create(World world) => new(world);
        public static implicit operator Query<C0>(in Filter<TFilters> filter) => filter.query;
        static Filter() => QueryFilterFetch<Filter<TFilters>>.Register();

        Query<C0> query;

        public Filter(World world)
        {
            query = new Query<C0>(world, QueryFilter<TFilters>.FilterArchetype, QueryFilter<TFilters>.FilterChunk);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach(ForEachDelegate<C0> pred)
        {
            query.ForEach(pred);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TUserData>(scoped in TUserData userData, ForEachUserDataDelegate<TUserData, C0> pred)
        {
            query.ForEach(userData, pred);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach(ForEachEntityDelegate<C0> pred)
        {
            query.ForEach(pred);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TUserData>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData, C0> pred)
        {
            query.ForEach(userData, pred);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TForEach>(TForEach iter)
                where TForEach : struct, IForEach<C0>
        {
            query.ForEach(iter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEachChunk<TForEach>(TForEach pred)
                where TForEach : struct, IChunkForEach<C0>
        {
            query.ForEachChunk(pred);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEachRawChunk<TForEach>(TForEach pred)
                where TForEach : struct, IRawChunkForEach
        {
            query.ForEachRawChunk(pred);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityRow Single()
        {
            return query.Single();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EntityCount()
        {
            return query.EntityCount();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(query);
        }
    }

    static readonly Component.Info[] infos = [Component.Register<C0>()];
    static readonly ComponentID[] cids = [infos[0].ID];
    public static Component.Info[] Infos => infos;

    static Query<C0> IQueryCreate<Query<C0>>.Create(World world) => new(world);

    static Query()
    {
        QueryFetch<Query<C0>>.Register();
    }

    readonly World world;
    FilterArchetypeDelegate? filterArchetype;
    FilterChunkDelegate? filterChunk;

    public Query(World world, FilterArchetypeDelegate? filterArchetype = null, FilterChunkDelegate? filterChunk = null)
    {
        this.world = world;
        this.filterArchetype = filterArchetype;
        this.filterChunk = filterChunk;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C0> Filtered<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForEach<TFilters>(ForEachDelegate<C0> pred)
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(pred);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ForEach(ForEachDelegate<C0> pred)
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped ref var curr = ref chunk.GetComponent<C0>(0, cids[0]);
            while (count-- > 0)
            {
                pred(ref curr);
                curr = ref Unsafe.Add(ref curr, 1);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ForEach<TUserData>(scoped in TUserData userData, ForEachUserDataDelegate<TUserData, C0> pred)
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped ref var curr = ref chunk.GetComponent<C0>(0, cids[0]);
            scoped ref var end = ref Unsafe.Add(ref curr, count);
            while (Unsafe.IsAddressLessThan(ref curr, ref end))
            {
                pred(in userData, ref curr);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForEach<TFilters>(ForEachEntityDelegate<C0> pred)
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(pred);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ForEach(ForEachEntityDelegate<C0> pred)
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped ref var curr = ref chunk.GetComponent<C0>(0, cids[0]);
            scoped ref var ent = ref chunk.GetEntity(0);
            while (count-- > 0)
            {
                pred(ent, ref curr);
                ent = ref Unsafe.Add(ref ent, 1);
                curr = ref Unsafe.Add(ref curr, 1);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ForEach<TUserData>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData, C0> pred)
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped ref var curr = ref chunk.GetComponent<C0>(0, cids[0]);
            scoped ref var ent = ref chunk.GetEntity(0);
            while (count-- > 0)
            {
                pred(in userData, ent, ref curr);
                ent = ref Unsafe.Add(ref ent, 1);
                curr = ref Unsafe.Add(ref curr, 1);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForEach<TForEach, TFilters>(TForEach iter)
        where TForEach : struct, IForEach<C0>
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(iter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ForEach<TForEach>(TForEach iter)
        where TForEach : struct, IForEach<C0>
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped ref var curr = ref chunk.GetComponent<C0>(0, cids[0]);
            scoped ref var ent = ref chunk.GetEntity(0);
            while (count-- > 0)
            {
                iter.Execute(ent, ref curr);
                curr = ref Unsafe.Add(ref curr, 1);
                ent = ref Unsafe.Add(ref ent, 1);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ForEachChunk<TForEach>(TForEach iter)
        where TForEach : struct, IChunkForEach<C0>
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            scoped var comp1 = chunk.GetComponents<C0>(cids[0]);
            iter.Execute(chunk.GetEntities(), comp1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForEachRawChunk<TForEach>(TForEach pred)
        where TForEach : IRawChunkForEach
    {
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            pred.Execute(in chunk);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EntityCount<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return EntityCount();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EntityCount()
    {
        int count = 0;
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            count += chunk.Count;
        }

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Any<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Any()
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            if (chunk.Count > 0) return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityRow Single<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return Single();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityRow Single()
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            return new EntityRow
            {
                entity = ref chunk.GetEntity(0),
                Component0 = ref chunk.GetComponents<C0>(cids[0])[0],
            };
        }

        throw new InvalidOperationException("No entities found");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ref struct Enumerator
    {
        ArchetypeChunkEnumerable chunks;
        ArchetypeChunkEnumerable.ChunkEnumerator chunksEnumerator;
        ref Entity endEntity;
        EntityRow currentRow;

        public EntityRow Current => currentRow;

        public Enumerator(scoped in Query<C0> query)
        {
            chunks = new ArchetypeChunkEnumerable(query.world.Store.Archetypes, cids, query.filterArchetype, query.filterChunk);
            chunksEnumerator = chunks.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (!Unsafe.IsNullRef(ref currentRow.entity) && Unsafe.IsAddressLessThan(ref currentRow.entity, ref endEntity))
            {
                currentRow.entity = ref Unsafe.Add(ref currentRow.entity, 1);
                currentRow.Component0 = ref Unsafe.Add(ref currentRow.Component0, 1);
                return true;
            }

            if (!chunksEnumerator.MoveNext()) return false;

            scoped ref var currentChunk = ref chunksEnumerator.Current;
            currentRow.entity = ref currentChunk.GetEntity(0);
            endEntity = ref Unsafe.Add(ref currentRow.entity, currentChunk.Count - 1);
            currentRow.Component0 = ref currentChunk.GetComponent<C0>(0, cids[0]);
            return true;
        }
    }

    public ref struct EntityRow
    {
        internal ref Entity entity;
        public ref C0 Component0;
        public readonly ref readonly Entity Entity => ref entity;
    }
}