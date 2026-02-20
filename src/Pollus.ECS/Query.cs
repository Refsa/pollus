namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public interface IQuery
{
    static abstract Component.Info[] Infos { get; }
    int EntityCount();
}

public interface IQueryCreate<out TQuery>
    where TQuery : IQuery
{
    static abstract TQuery Create(World world);
}

public class QueryFetch<TQuery> : IFetch<TQuery>
    where TQuery : IQuery, IQueryCreate<TQuery>
{
    public static void Register()
    {
        Fetch.Register(new QueryFetch<TQuery>(), [.. TQuery.Infos.Where(e => e.Write).Select(e => e.Type), typeof(Query)]);
    }

    public TQuery DoFetch(World world, ISystem system)
    {
        return TQuery.Create(world);
    }
}

public class QueryFilterFetch<TFilterQuery> : IFetch<TFilterQuery>
    where TFilterQuery : IQuery, IQueryCreate<TFilterQuery>
{
    public static void Register()
    {
        Fetch.Register(new QueryFilterFetch<TFilterQuery>(), [typeof(TFilterQuery)]);
    }

    public TFilterQuery DoFetch(World world, ISystem system)
    {
        if (!system.Resources.TryGet<TFilterQuery>(out var query))
        {
            query = TFilterQuery.Create(world);
            system.Resources.Add(query);
        }

        return query;
    }
}

public struct Query : IQuery, IQueryCreate<Query>
{
    public class Filter<TFilters> : IQuery, IQueryCreate<Filter<TFilters>>
        where TFilters : ITuple, IFilter, new()
    {
        public static Component.Info[] Infos => infos;
        public static Filter<TFilters> Create(World world) => new(world);
        public static implicit operator Query(in Filter<TFilters> filter) => filter.query;
        static Filter() => QueryFilterFetch<Filter<TFilters>>.Register();

        Query query;

        public Filter(World world)
        {
            query = new Query(world, QueryFilter<TFilters>.FilterArchetype, QueryFilter<TFilters>.FilterChunk);
        }

        public void ForEach(ForEachEntityDelegate pred) => query.ForEach(pred);
        public void ForEach<TUserData>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData> pred) => query.ForEach(userData, pred);
        public void ForEachChunk<TForEach>(TForEach pred) where TForEach : IRawChunkForEach => query.ForEachChunk(pred);
        public Entity Single() => query.Single();
        public int EntityCount() => query.EntityCount();

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
    FilterArchetypeDelegate? filterArchetype;
    FilterChunkDelegate? filterChunk;

    public Query(World world, FilterArchetypeDelegate? filterArchetype = null, FilterChunkDelegate? filterChunk = null)
    {
        this.world = world;
        this.filterArchetype = filterArchetype;
        this.filterChunk = filterChunk;
    }

    public Query Filtered<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return this;
    }

    public void ForEach<TFilters>(ForEachEntityDelegate pred)
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(pred);
    }

    public void ForEach(ForEachEntityDelegate pred)
    {
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, filterArchetype, filterChunk))
        {
            var entities = chunk.GetEntities();
            for (int i = 0; i < chunk.Count; i++)
            {
                pred(entities[i]);
            }
        }
    }

    public void ForEach<TUserData, TFilters>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData> pred)
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        ForEach(userData, pred);
    }

    public void ForEach<TUserData>(scoped in TUserData userData, ForEachEntityUserDataDelegate<TUserData> pred)
    {
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, filterArchetype, filterChunk))
        {
            var entities = chunk.GetEntities();
            for (int i = 0; i < chunk.Count; i++)
            {
                pred(userData, entities[i]);
            }
        }
    }

    public void ForEachChunk<TForEach>(TForEach pred)
        where TForEach : IRawChunkForEach
    {
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, filterArchetype, filterChunk))
        {
            pred.Execute(in chunk);
        }
    }

    public Entity Single<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return Single();
    }

    public Entity Single()
    {
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, filterArchetype, filterChunk))
        {
            return chunk.GetEntities()[0];
        }

        throw new InvalidOperationException("No entities found");
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
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, filterArchetype, filterChunk))
        {
            count += chunk.Count;
        }

        return count;
    }

    public bool Any<TFilters>()
        where TFilters : ITuple, new()
    {
        filterArchetype = QueryFilter<TFilters>.FilterArchetype;
        filterChunk = QueryFilter<TFilters>.FilterChunk;
        return Any();
    }

    public bool Any()
    {
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, filterArchetype, filterChunk))
        {
            if (chunk.Count > 0) return true;
        }

        return false;
    }

    /// <summary>
    /// Check if entity has component
    /// </summary>
    /// <param name="entity">entity to check</param>
    /// <typeparam name="C">component to check</typeparam>
    /// <returns>true if it has component</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        if (entityInfo.Entity.Version != entity.Version) return false;
        return world.Store.Archetypes[entityInfo.ArchetypeIndex].HasComponent<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<C>(scoped in Entity entity, out Entities.EntityInfo entityInfo)
        where C : unmanaged, IComponent
    {
        entityInfo = world.Store.GetEntityInfo(entity);
        if (entityInfo.Entity.Version != entity.Version) return false;
        return world.Store.Archetypes[entityInfo.ArchetypeIndex].HasComponent<C>();
    }

    /// <summary>
    /// Check if a component has been added to an entity
    /// </summary>
    /// <param name="entity">entity to check</param>
    /// <typeparam name="C">component to check</typeparam>
    /// <returns>true if it was added</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Added<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        if (entityInfo.Entity.Version != entity.Version) return false;
        return world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].HasFlag<C>(entityInfo.RowIndex, ComponentFlags.Added);
    }

    /// <summary>
    /// Check if a component has been changed on an entity
    /// </summary>
    /// <param name="entity">entity to check</param>
    /// <typeparam name="C">component to check</typeparam>
    /// <returns>true if was changes</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Changed<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        if (entityInfo.Entity.Version != entity.Version) return false;
        return world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].HasFlag<C>(entityInfo.RowIndex, ComponentFlags.Changed);
    }

    /// <summary>
    /// Set a component as changed. 
    /// This is a relatively expensive operation and should be used sparingly.
    /// </summary>
    /// <param name="entity">entity to mark</param>
    /// <typeparam name="C">component to mark</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetChanged<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        if (entityInfo.Entity.Version != entity.Version) return;
        world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].SetFlag<C>(entityInfo.RowIndex, ComponentFlags.Changed);
    }

    /// <summary>
    /// Check if a component has been removed from an entity
    /// </summary>
    /// <param name="entity">entity to check</param>
    /// <typeparam name="C">component to check</typeparam>
    /// <returns>true if component was removed</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Removed<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        return world.Store.Removed.WasRemoved<C>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref C Get<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        return ref Get<C>(world.Store.GetEntityInfo(entity));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref C Get<C>(scoped in Entities.EntityInfo entityInfo)
        where C : unmanaged, IComponent
    {
        return ref world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex].GetComponent<C>(entityInfo.RowIndex);
    }

    /// <summary>
    /// Get a mutable reference to a component and mark it as changed.
    /// Use this instead of Get when you intend to modify the component and want change detection to work.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C GetTracked<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        ref var chunk = ref world.Store.GetArchetype(entityInfo.ArchetypeIndex).Chunks[entityInfo.ChunkIndex];
        chunk.SetFlag<C>(entityInfo.RowIndex, ComponentFlags.Changed);
        return ref chunk.GetComponent<C>(entityInfo.RowIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref C TryGet<C>(in Entity entity, out bool exists)
        where C : unmanaged, IComponent
    {
        if (Has<C>(entity, out var entityInfo))
        {
            exists = true;
            return ref Get<C>(entityInfo);
        }

        exists = false;
        return ref Unsafe.NullRef<C>();
    }

    /// <summary>
    /// Check if any entity in the world has the Changed flag set for a component.
    /// Uses chunk-level flags for a fast early exit without per-entity iteration.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool AnyChanged<C>()
        where C : unmanaged, IComponent
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, filterArchetype, filterChunk))
        {
            if (chunk.Count > 0 && chunk.HasFlag<C>(-1, ComponentFlags.Changed))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check if any entity in the world has the Added flag set for a component.
    /// Uses chunk-level flags for a fast early exit without per-entity iteration.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool AnyAdded<C>()
        where C : unmanaged, IComponent
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, filterArchetype, filterChunk))
        {
            if (chunk.Count > 0 && chunk.HasFlag<C>(-1, ComponentFlags.Added))
                return true;
        }
        return false;
    }

    public readonly EntityRef GetEntity(in Entity entity)
    {
        return world.GetEntityRef(entity);
    }

    public ComponentID[] GetComponents(in Entity entity)
    {
        ref var entityInfo = ref world.Store.GetEntityInfo(entity);
        var archetype = world.Store.GetArchetype(entityInfo.ArchetypeIndex);
        return archetype.GetChunkInfo().ComponentIDs;
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        ArchetypeChunkEnumerable.ChunkEnumerator chunksEnumerator;
        ref Entity currentEntity;
        ref Entity endEntity;
        public Entity Current => currentEntity;

        public Enumerator(scoped in Query query)
        {
            var chunks = new ArchetypeChunkEnumerable(query.world.Store.Archetypes, query.filterArchetype, query.filterChunk);
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