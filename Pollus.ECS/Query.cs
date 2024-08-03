namespace Pollus.ECS;
using System.Runtime.CompilerServices;

public interface IQuery
{
    static abstract Component.Info[] Infos { get; }
}

public struct Query<C0> : IQuery
    where C0 : unmanaged, IComponent
{
    public struct Filter<TFilters> : IQuery
        where TFilters : ITuple
    {
        public static Component.Info[] Infos => infos;
        static readonly IFilter[] filters;
        static FilterDelegate filterDelegate = RunFilter;

        static Filter()
        {
            filters = FilterHelpers.UnwrapFilters<TFilters>();
        }

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
            where TForEach : unmanaged, IForEachBase<C0>
        {
            query.ForEach(iter);
        }
    }

    static readonly Component.Info[] infos = [Component.Register<C0>()];
    public static Component.Info[] Infos => infos;

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
        where TForEach : unmanaged, IForEachBase<C0>
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
}
