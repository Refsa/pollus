namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public struct Query<C0> : IQuery, IQueryCreate<Query<C0>>
    where C0 : unmanaged, IComponent
{
    public struct Filter<TFilters> : IQuery, IQueryCreate<Filter<TFilters>>
        where TFilters : ITuple, new()
    {
        public static Component.Info[] Infos => infos;
        public static Filter<TFilters> Create(World world) => new(world);
        public static implicit operator Query<C0>(in Filter<TFilters> filter) => filter.query;
        static Filter() => QueryFetch<Filter<TFilters>>.Register();

        Query<C0> query;

        public Filter(World world)
        {
            query = new Query<C0>(world, QueryFilter<TFilters>.FilterArchetype, QueryFilter<TFilters>.FilterChunk);
        }

        public void ForEach(ForEachDelegate<C0> pred)
        {
            query.ForEach(pred);
        }

        public void ForEach<TUserData>(scoped in TUserData userData, ForEachUserDataDelegate<TUserData, C0> pred)
        {
            query.ForEach(userData, pred);
        }

        public void ForEach(ForEachEntityDelegate<C0> pred)
        {
            query.ForEach(pred);
        }

        public void ForEach<TUserData>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData, C0> pred)
        {
            query.ForEach(userData, pred);
        }

        public readonly void ForEach<TForEach>(TForEach iter)
            where TForEach : struct, IForEachBase<C0>
        {
            query.ForEach(iter);
        }

        public void ForEachChunk<TForEach>(TForEach pred)
            where TForEach : IRawChunkForEach
        {
            query.ForEachChunk(pred);
        }

        public EntityRow Single()
        {
            return query.Single();
        }

        public int EntityCount()
        {
            return query.EntityCount();
        }

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

    public void ForEach<TFilters>(ForEachDelegate<C0> pred)
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(pred);
    }

    public readonly void ForEach(ForEachDelegate<C0> pred)
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped ref var curr = ref chunk.GetComponent<C0>(0, cids[0]);
            scoped ref var end = ref Unsafe.Add(ref curr, count);
            while (Unsafe.IsAddressLessThan(ref curr, ref end))
            {
                pred(ref curr);
                curr = ref Unsafe.Add(ref curr, 1);
            }
        }
    }

    public void ForEach<TUserData, TFilters>(scoped in TUserData userData, ForEachUserDataDelegate<TUserData, C0> pred)
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(userData, pred);
    }

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
                curr = ref Unsafe.Add(ref curr, 1);
            }
        }
    }
    public void ForEach<TFilters>(ForEachEntityDelegate<C0> pred)
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(pred);
    }

    public readonly void ForEach(ForEachEntityDelegate<C0> pred)
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped ref var curr = ref chunk.GetComponent<C0>(0, cids[0]);
            scoped ref var end = ref Unsafe.Add(ref curr, count);
            scoped ref var ent = ref chunk.GetEntity(0);
            while (Unsafe.IsAddressLessThan(ref curr, ref end))
            {
                pred(ent, ref curr);
                ent = ref Unsafe.Add(ref ent, 1);
                curr = ref Unsafe.Add(ref curr, 1);
            }
        }
    }

    public void ForEach<TUserData, TFilters>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData, C0> pred)
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(userData, pred);
    }

    public readonly void ForEach<TUserData>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData, C0> pred)
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped ref var curr = ref chunk.GetComponent<C0>(0, cids[0]);
            scoped ref var end = ref Unsafe.Add(ref curr, count);
            scoped ref var ent = ref chunk.GetEntity(0);
            while (Unsafe.IsAddressLessThan(ref curr, ref end))
            {
                pred(in userData, ent, ref curr);
                ent = ref Unsafe.Add(ref ent, 1);
                curr = ref Unsafe.Add(ref curr, 1);
            }
        }
    }

    public void ForEach<TForEach, TFilters>(TForEach iter)
        where TForEach : struct, IForEachBase<C0>
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(iter);
    }

    public readonly void ForEach<TForEach>(TForEach iter)
        where TForEach : struct, IForEachBase<C0>
    {
        foreach (scoped ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            scoped var comp1 = chunk.GetComponents<C0>(cids[0]);

            if (iter is IForEach<C0>)
            {
                scoped ref var curr = ref comp1[0];
                for (int i = 0; i < count; i++, curr = ref Unsafe.Add(ref curr, 1))
                {
                    iter.Execute(ref curr);
                }
            }
            else if (iter is IEntityForEach<C0>)
            {
                scoped ref var curr = ref comp1[0];
                scoped ref var ent = ref chunk.GetEntity(0);

                for (int i = 0; i < count; i++, curr = ref Unsafe.Add(ref curr, 1), ent = ref Unsafe.Add(ref ent, 1))
                {
                    iter.Execute(ent, ref curr);
                }
            }
            else if (iter is IChunkForEach<C0>)
            {
                iter.Execute(comp1);
            }
            else if (iter is IChunkEntityForEach<C0>)
            {
                scoped var entities = chunk.GetEntities();
                iter.Execute(entities, comp1);
            }
        }
    }

    public void ForEachChunk<TForEach>(TForEach pred)
        where TForEach : IRawChunkForEach
    {
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            pred.Execute(in chunk);
        }
    }

    public int EntityCount<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return EntityCount();
    }

    public int EntityCount()
    {
        int count = 0;
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            count += chunk.Count;
        }
        return count;
    }

    public EntityRow Single<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return Single();
    }

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

        public Enumerator(scoped in Query<C0> query)
        {
            chunks = new ArchetypeChunkEnumerable(query.world.Store.Archetypes, cids, query.filterArchetype, query.filterChunk);
            chunksEnumerator = chunks.GetEnumerator();
        }

        public EntityRow Current => currentRow;

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
            currentRow.Component0 = ref currentChunk.GetComponents<C0>(cids[0])[0];
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