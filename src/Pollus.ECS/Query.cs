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

        public void ForEach<TUserData>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData> pred)
        {
            query.ForEach(userData, pred);
        }

        public Entity Single()
        {
            return query.Single();
        }

        public int EntityCount()
        {
            return query.EntityCount();
        }

        public Enumerator GetEnumerator() => new(query);
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
        scoped ReadOnlySpan<ComponentID> cids = stackalloc ComponentID[0];
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var entities = chunk.GetEntities();
            for (int i = 0; i < chunk.Count; i++)
            {
                pred(entities[i]);
            }
        }
    }

    public void ForEach<TUserData>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData> pred)
    {
        scoped ReadOnlySpan<ComponentID> cids = stackalloc ComponentID[0];
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var entities = chunk.GetEntities();
            for (int i = 0; i < chunk.Count; i++)
            {
                pred(userData, entities[i]);
            }
        }
    }

    public Entity Single()
    {
        scoped ReadOnlySpan<ComponentID> cids = stackalloc ComponentID[0];
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            return chunk.GetEntities()[0];
        }
        throw new InvalidOperationException("No entities found");
    }

    public int EntityCount()
    {
        int count = 0;
        scoped ReadOnlySpan<ComponentID> cids = stackalloc ComponentID[0];
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
        var entityInfo = world.Store.GetEntityInfo(entity);
        return world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].HasFlag<C>(entityInfo.RowIndex, ComponentFlags.Added);
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
        world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].SetFlag<C>(entityInfo.RowIndex, ComponentFlags.Changed);
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
        return world.Store.Changes.WasRemoved<C>(entity);
    }

    public ref C Get<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        return ref world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].GetComponent<C>(entityInfo.RowIndex);
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        ArchetypeChunkEnumerable chunks;
        ArchetypeChunkEnumerable.ChunkEnumerator chunksEnumerator;
        ref Entity currentEntity;
        ref Entity endEntity;
        public Entity Current => currentEntity;

        public Enumerator(scoped in Query query)
        {
            chunks = new ArchetypeChunkEnumerable(query.world.Store.Archetypes, [], query.filterArchetype, query.filterChunk);
            chunksEnumerator = chunks.GetEnumerator();
        }

        public bool MoveNext()
        {
            if (!Unsafe.IsNullRef(ref currentEntity) && Unsafe.IsAddressLessThan(ref currentEntity, ref endEntity))
            {
                currentEntity = ref Unsafe.Add(ref currentEntity, 1);
                return true;
            }

            if (!chunksEnumerator.MoveNext()) return false;

            scoped ref var currentChunk = ref chunksEnumerator.Current;
            currentEntity = ref currentChunk.GetEntity(0);
            endEntity = ref Unsafe.Add(ref currentEntity, currentChunk.Count - 1);
            return true;
        }
    }
}