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
            if (typeof(TFilters).IsAssignableFrom(typeof(ITuple)) is false)
            {
                if (typeof(TFilters).IsAssignableTo(typeof(IFilter)) is false)
                {
                    throw new ArgumentException("Type must implement IFilter");
                }
                filters = [(IFilter)Activator.CreateInstance<TFilters>()!];
            }
            else
            {
                var types = typeof(TFilters).GetGenericArguments();
                var length = types.Length;
                filters = new IFilter[length];

                for (int i = 0; i < length; i++)
                {
                    if (types[i].IsAssignableTo(typeof(IFilter)) is false)
                    {
                        throw new ArgumentException("Type must implement IFilter");
                    }

                    filters[i] = (IFilter)Activator.CreateInstance(types[i])!;
                }
            }
        }

        static bool RunFilter(Archetype archetype)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                if (filters[i].Filter(archetype) is false) return false;
            }

            return true;
        }

        public static implicit operator Query<C0>(in Filter<TFilters> filter)
        {
            return filter.query;
        }

        Query<C0> query;

        public Filter(World world)
        {
            query = new Query<C0>(world, filterDelegate);
        }

        public void ForEach(IterDelegate<C0> pred)
        {
            query.ForEach(pred);
        }
    }

    static readonly Component.Info[] infos = [Component.GetInfo<C0>()];
    public static Component.Info[] Infos => infos;

    static Query()
    {
        Component.Register<C0>();
    }

    readonly World world;
    readonly FilterDelegate? filter;

    public Query(World world, FilterDelegate? filter = null)
    {
        this.world = world;
        this.filter = filter;
    }

    public readonly void ForEach(IterDelegate<C0> pred)
    {
        Span<ComponentID> cids = stackalloc ComponentID[1] { infos[0].ID };

        foreach (var archetype in world.Store.Archetypes)
        {
            if (archetype.HasComponents(cids) is false) continue;
            if (filter != null && filter(archetype) is false) continue;

            var chunks = archetype.Chunks;
            foreach (var chunk in archetype.Chunks)
            {
                var comp1 = chunk.GetComponents<C0>(cids[0]);
                foreach (ref var curr in comp1)
                {
                    pred(ref curr);
                }
            }
        }
    }

    public readonly void ForEach<TForEach>(TForEach iter)
        where TForEach : unmanaged, IForEachBase<C0>
    {
        Span<ComponentID> cids = stackalloc ComponentID[1] { infos[0].ID };
        foreach (var archetype in world.Store.Archetypes)
        {
            if (archetype.HasComponents(cids) is false) continue;
            if (filter != null && filter(archetype) is false) continue;

            foreach (var chunk in archetype.Chunks)
            {
                var comp1 = chunk.GetComponents<C0>(cids[0]);

                if (iter is IForEach<C0>)
                {
                    ref var curr = ref comp1[0];
                    for (int i = 0; i < chunk.Count; i++, curr = ref Unsafe.Add(ref curr, 1))
                    {
                        iter.Execute(ref curr);
                    }
                }
                else if (iter is IEntityForEach<C0>)
                {
                    var entities = chunk.GetEntities();
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
}

public interface IFilter : ITuple
{
    bool Filter(Archetype archetype);
}

public class With<C0> : IFilter
    where C0 : unmanaged, IComponent
{
    static With() => Component.Register<C0>();

    public object? this[int index] => null;
    public int Length => 1;

    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent<C0>() is true;
    }
}

public class Without<C0> : IFilter
    where C0 : unmanaged, IComponent
{
    static Without() => Component.Register<C0>();

    public object? this[int index] => null;
    public int Length => 1;

    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent<C0>() is false;
    }
}
