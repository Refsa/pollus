namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public interface IQuery
{
    static abstract Component.Info[] Infos { get; }
}

public interface IQueryCreate<TQuery>
    where TQuery : struct, IQuery
{
    static abstract TQuery Create(World world);
}

public class QueryFetch<TQuery> : IFetch<TQuery>
    where TQuery : struct, IQuery, IQueryCreate<TQuery>
{
    public static void Register()
    {
        Fetch.Register(new QueryFetch<TQuery>(), [.. TQuery.Infos.Select(e => e.Type)]);
    }

    public TQuery DoFetch(World world, ISystem system)
    {
        return TQuery.Create(world);
    }
}

public struct Query : IQuery, IQueryCreate<Query>
{
    public struct Filter<TFilters> : IQuery, IQueryCreate<Filter<TFilters>>
        where TFilters : ITuple, new()
    {
        public static Component.Info[] Infos => infos;
        static readonly IFilter[] filters;
        static FilterArchetypeDelegate filterArchetype = RunArchetypeFilter;
        static FilterChunkDelegate filterChunk = RunChunkFilter;

        public static implicit operator Query(in Filter<TFilters> filter) => filter.query;
        static bool RunArchetypeFilter(Archetype archetype) => FilterHelpers.RunArchetypeFilters(archetype, filters);
        static bool RunChunkFilter(in ArchetypeChunk chunk) => FilterHelpers.RunChunkFilters(chunk, filters);
        public static Filter<TFilters> Create(World world) => new Filter<TFilters>(world);

        static Filter()
        {
            filters = FilterHelpers.UnwrapFilters<TFilters>();
            QueryFetch<Filter<TFilters>>.Register();
        }

        Query query;

        public Filter(World world)
        {
            query = new Query(world, filterArchetype, filterChunk);
        }

        public void ForEach(ForEachEntityDelegate pred)
        {
            query.ForEach(pred);
        }

        public Entity Single()
        {
            return query.Single();
        }

        public int EntityCount()
        {
            return query.EntityCount();
        }
    }

    static readonly Component.Info[] infos = [];
    public static Component.Info[] Infos => infos;

    static Query()
    {
        QueryFetch<Query>.Register();
    }

    public static Query Create(World world)
    {
        return new Query(world);
    }

    World world;
    readonly FilterArchetypeDelegate? filterArchetype;
    readonly FilterChunkDelegate? filterChunk;

    public Query(World world, FilterArchetypeDelegate? filterArchetype = null, FilterChunkDelegate? filterChunk = null)
    {
        this.world = world;
        this.filterArchetype = filterArchetype;
        this.filterChunk = filterChunk;
    }

    public void ForEach(ForEachEntityDelegate pred)
    {
        scoped Span<ComponentID> cids = stackalloc ComponentID[0];
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var entities = chunk.GetEntities();
            for (int i = 0; i < chunk.Count; i++)
            {
                pred(entities[i]);
            }
        }
    }

    public Entity Single()
    {
        scoped Span<ComponentID> cids = stackalloc ComponentID[0];
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            return chunk.GetEntities()[0];
        }
        throw new InvalidOperationException("No entities found");
    }

    public int EntityCount()
    {
        int count = 0;
        scoped Span<ComponentID> cids = stackalloc ComponentID[0];
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            count += chunk.Count;
        }
        return count;
    }

    /// <summary>
    /// Check if entity has component
    /// </summary>
    /// <param name="entity">entity to check</param>
    /// <typeparam name="C">component to check</typeparam>
    /// <returns>true if it has component</returns>
    public readonly bool Has<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        return world.Store.Archetypes[entityInfo.ArchetypeIndex].HasComponent<C>();
    }

    /// <summary>
    /// Check if a component has been added to an entity
    /// </summary>
    /// <param name="entity">entity to check</param>
    /// <typeparam name="C">component to check</typeparam>
    /// <returns>true if it was added</returns>
    public readonly bool Added<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        return world.Store.Changes.HasFlag<C>(entity, ComponentFlags.Added);
    }

    /// <summary>
    /// Check if a component has been changed on an entity
    /// </summary>
    /// <param name="entity">entity to check</param>
    /// <typeparam name="C">component to check</typeparam>
    /// <returns>true if was changes</returns>
    public readonly bool Changed<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        return world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].HasFlag<C>(entityInfo.RowIndex, ComponentFlags.Changed);
    }

    /// <summary>
    /// Set a component as changed. 
    /// This is a relatively expensive operation and should be used sparingly.
    /// </summary>
    /// <param name="entity">entity to mark</param>
    /// <typeparam name="C">component to mark</typeparam>
    public void SetChanged<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].SetFlag<C>(ComponentFlags.Changed, entityInfo.RowIndex);
    }

    /// <summary>
    /// Check if a component has been removed from an entity
    /// </summary>
    /// <param name="entity">entity to check</param>
    /// <typeparam name="C">component to check</typeparam>
    /// <returns>true if component was removed</returns>
    public readonly bool Removed<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        return world.Store.Changes.HasFlag<C>(entity, ComponentFlags.Removed);
    }

    public ref C Get<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        return ref world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].GetComponent<C>(entityInfo.RowIndex);
    }
}

public struct Query<C0> : IQuery, IQueryCreate<Query<C0>>
    where C0 : unmanaged, IComponent
{
    public struct Filter<TFilters> : IQuery, IQueryCreate<Filter<TFilters>>
        where TFilters : ITuple, new()
    {
        public static Component.Info[] Infos => infos;
        static readonly IFilter[] filters;
        static FilterArchetypeDelegate filterDelegate = RunArchetypeFilter;
        static FilterChunkDelegate filterChunkDelegate = RunChunkFilter;

        static Filter()
        {
            filters = FilterHelpers.UnwrapFilters<TFilters>();
            QueryFetch<Filter<TFilters>>.Register();
        }

        public static Filter<TFilters> Create(World world) => new(world);

        static bool RunArchetypeFilter(Archetype archetype) => FilterHelpers.RunArchetypeFilters(archetype, filters);
        static bool RunChunkFilter(in ArchetypeChunk chunk) => FilterHelpers.RunChunkFilters(chunk, filters);

        public static implicit operator Query<C0>(in Filter<TFilters> filter)
        {
            return filter.query;
        }

        Query<C0> query;

        public Filter(World world)
        {
            query = new Query<C0>(world, filterDelegate, filterChunkDelegate);
        }

        public void ForEach(ForEachDelegate<C0> pred)
        {
            query.ForEach(pred);
        }

        public void ForEach(ForEachEntityDelegate<C0> pred)
        {
            query.ForEach(pred);
        }

        public readonly void ForEach<TForEach>(TForEach iter)
            where TForEach : struct, IForEachBase<C0>
        {
            query.ForEach(iter);
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
    readonly FilterArchetypeDelegate? filterArchetype;
    readonly FilterChunkDelegate? filterChunk;

    public Query(World world, FilterArchetypeDelegate? filterArchetype = null, FilterChunkDelegate? filterChunk = null)
    {
        this.world = world;
        this.filterArchetype = filterArchetype;
        this.filterChunk = filterChunk;
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
        }
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

    public EntityRow Single()
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            return new EntityRow
            {
                Entity = chunk.GetEntities()[0],
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
        ref Entity currentEntity;
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
            if (!Unsafe.IsNullRef(ref currentEntity) && Unsafe.IsAddressLessThan(ref currentEntity, ref endEntity))
            {
                currentEntity = ref Unsafe.Add(ref currentEntity, 1);
                currentRow.Component0 = ref Unsafe.Add(ref currentRow.Component0, 1);
                currentRow.Entity = currentEntity;
                return true;
            }

            if (!chunksEnumerator.MoveNext()) return false;

            scoped ref var currentChunk = ref chunksEnumerator.Current;
            currentEntity = ref currentChunk.GetEntity(0);
            endEntity = ref Unsafe.Add(ref currentEntity, currentChunk.Count - 1);
            currentRow.Component0 = ref currentChunk.GetComponents<C0>(cids[0])[0];
            currentRow.Entity = currentEntity;
            return true;
        }
    }

    public ref struct EntityRow
    {
        public Entity Entity;
        public ref C0 Component0;
    }
}