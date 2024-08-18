namespace Pollus.ECS;
using System.Runtime.CompilerServices;
using Pollus.ECS.Core;

public interface IQuery
{
    static abstract Component.Info[] Infos { get; }
}

public interface IQueryCreate<TQuery>
    where TQuery : struct, IQuery
{
    static abstract TQuery Create(World world);
}

public struct Query<C0> : IQuery, IQueryCreate<Query<C0>>
    where C0 : unmanaged, IComponent
{
    public struct Filter<TFilters> : IQuery, IQueryCreate<Filter<TFilters>>
        where TFilters : ITuple, new()
    {
        public static Component.Info[] Infos => infos;
        static readonly IFilter[] filters;
        static FilterDelegate filterDelegate = RunFilter;

        static Filter()
        {
            filters = FilterHelpers.UnwrapFilters<TFilters>();
            QueryFetch<Filter<TFilters>>.Register();
        }

        public static Filter<TFilters> Create(World world) => new Filter<TFilters>(world);

        static bool RunFilter(Archetype archetype) => FilterHelpers.RunFilters(archetype, filters);

        public static implicit operator Query<C0>(in Filter<TFilters> filter)
        {
            return filter.query;
        }

        Query<C0> query;

        public Filter(World world)
        {
            query = new Query<C0>(world, filterDelegate);
        }

        public void ForEach(ForEachDelegate<C0> pred)
        {
            query.ForEach(pred);
        }

        public readonly void ForEach<TForEach>(TForEach iter)
            where TForEach : struct, IForEachBase<C0>
        {
            query.ForEach(iter);
        }
    }

    static readonly Component.Info[] infos = [Component.Register<C0>()];
    public static Component.Info[] Infos => infos;

    static Query<C0> IQueryCreate<Query<C0>>.Create(World world) => new Query<C0>(world);
    static Query()
    {
        QueryFetch<Query<C0>>.Register();
    }

    readonly World world;
    readonly FilterDelegate? filter;

    public Query(World world, FilterDelegate? filter = null)
    {
        this.world = world;
        this.filter = filter;
    }

    public readonly void ForEach(ForEachDelegate<C0> pred)
    {
        scoped Span<ComponentID> cids = stackalloc ComponentID[1] { infos[0].ID };
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filter))
        {
            scoped var comp1 = chunk.GetComponents<C0>(cids[0]);
            foreach (ref var curr in comp1)
            {
                pred(ref curr);
            }
        }
    }

    public readonly void ForEach<TForEach>(TForEach iter)
        where TForEach : struct, IForEachBase<C0>
    {
        scoped Span<ComponentID> cids = stackalloc ComponentID[1] { infos[0].ID };
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filter))
        {
            scoped var comp1 = chunk.GetComponents<C0>(cids[0]);

            if (iter is IForEach<C0>)
            {
                scoped ref var curr = ref comp1[0];
                for (int i = 0; i < chunk.Count; i++, curr = ref Unsafe.Add(ref curr, 1))
                {
                    iter.Execute(ref curr);
                }
            }
            else if (iter is IEntityForEach<C0>)
            {
                scoped var entities = chunk.GetEntities();
                for (int i = 0; i < chunk.Count; i++)
                {
                    iter.Execute(entities[i], ref comp1[i]);
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
        scoped Span<ComponentID> cids = stackalloc ComponentID[1] { infos[0].ID };
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filter))
        {
            count += chunk.Count;
        }
        return count;
    }

    public EntityRow Single()
    {
        scoped Span<ComponentID> cids = stackalloc ComponentID[1] { infos[0].ID };
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filter))
        {
            return new EntityRow
            {
                Entity = chunk.GetEntities()[0],
                Component = ref chunk.GetComponents<C0>(cids[0])[0],
            };
        }
        throw new InvalidOperationException("No entities found");
    }

    public ref struct EntityRow
    {
        public Entity Entity;
        public ref C0 Component;
    }
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