namespace Pollus.ECS.Generators
{
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class QueryGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(GenerateForEach);
            context.RegisterPostInitializationOutput(GenerateQueries);
        }

        private void GenerateQueries(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

public struct Query<$gen_args$> : IQuery
    $gen_constraints$
{
    public struct Filter<TFilters> : IQuery
        where TFilters : ITuple
    {
        public static Component.Info[] Infos => infos;
        static readonly IFilter[] filters;
        static FilterDelegate filterDelegate = RunFilter;

        static Filter()
        {
            filters = FilterHelpers.Unwrap<TFilters>();
        }

        static bool RunFilter(Archetype archetype) => FilterHelpers.RunFilters(archetype, filters);

        public static implicit operator Query<$gen_args$>(in Filter<TFilters> filter)
        {
            return filter.query;
        }

        Query<$gen_args$> query;

        public Filter(World world)
        {
            query = new Query<$gen_args$>(world, filterDelegate);
        }

        public void ForEach(ForEachDelegate<$gen_args$> pred)
        {
            query.ForEach(pred);
        }

        public readonly void ForEach<TForEach>(TForEach iter)
            where TForEach : unmanaged, IForEachBase<$gen_args$>
        {
            query.ForEach(iter);
        }
    }

    static readonly Component.Info[] infos = [$infos$];
    public static Component.Info[] Infos => infos;

    readonly World world;
    readonly FilterDelegate? filter;

    public Query(World world, FilterDelegate? filter = null)
    {
        this.world = world;
        this.filter = filter;
    }

    public readonly void ForEach(ForEachDelegate<$gen_args$> pred)
    {
        scoped Span<ComponentID> cids = stackalloc ComponentID[$gen_count$] { $comp_ids$ };
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filter))
        {
            $comp_spans$
            
            for (int i = 0; i < chunk.Count; i++)
            {
                pred($comp_args$);
            }
        }
    }

    public readonly void ForEach<TForEach>(TForEach iter)
        where TForEach : unmanaged, IForEachBase<$gen_args$>
    {
        scoped Span<ComponentID> cids = stackalloc ComponentID[$gen_count$] { $comp_ids$ };
        foreach (var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filter))
        {
            $comp_spans$

            if (iter is IForEach<$gen_args$>)
            {
                for (int i = 0; i < chunk.Count; i++)
                {
                    iter.Execute($comp_args$);
                }
            }
            else if (iter is IEntityForEach<$gen_args$>)
            {
                scoped var entities = chunk.GetEntities();
                for (int i = 0; i < chunk.Count; i++)
                {
                    iter.Execute(entities[i], $comp_args$);
                }
            }
            else if (iter is IChunkForEach<$gen_args$>)
            {
                iter.Execute($chunk_args$);
            }
        }
    }
}";

            var sb = new StringBuilder();
            var gen_args = "C0";
            var gen_constraints = "where C0 : unmanaged, IComponent";
            var infos = "Component.Register<C0>()";
            var comp_ids = "infos[0].ID";
            var comp_args = "ref comp0[i]";
            var comp_spans = "scoped var comp0 = chunk.GetComponents<C0>(cids[0]);";
            var chunk_args = "comp0";

            for (int i = 1; i < 16; i++)
            {
                gen_args += $", C{i}";
                gen_constraints += $"\nwhere C{i} : unmanaged, IComponent";
                infos += $", Component.Register<C{i}>()";
                comp_ids += $", infos[{i}].ID";
                comp_args += $", ref comp{i}[i]";
                comp_spans += $"\nscoped var comp{i} = chunk.GetComponents<C{i}>(cids[{i}]);";
                chunk_args += $", comp{i}";

                sb.Clear().Append(TEMPLATE)
                    .Replace("$gen_args$", gen_args)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$infos$", infos)
                    .Replace("$comp_ids$", comp_ids)
                    .Replace("$gen_count$", (i + 1).ToString())
                    .Replace("$comp_spans$", comp_spans)
                    .Replace("$comp_args$", comp_args)
                    .Replace("$chunk_args$", chunk_args)
                    ;

                context.AddSource($"Query{i + 1}.gen.cs", sb.ToString());
            }
        }

        private void GenerateForEach(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

public delegate void ForEachDelegate<$gen_args$>($args$) 
    $gen_constraints$;

public interface IForEachBase<$gen_args$>
    $gen_constraints$
{
    void Execute($args$) { }
    void Execute(in Entity entity, $args$) { }
    void Execute($chunk_spans$) { }
}

public interface IForEach<$gen_args$> : IForEachBase<$gen_args$>
    $gen_constraints$
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute($args$);
}

public interface IEntityForEach<$gen_args$> : IForEachBase<$gen_args$>
    $gen_constraints$
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute(in Entity entity, $args$);
}

public interface IChunkForEach<$gen_args$> : IForEachBase<$gen_args$>
    $gen_constraints$
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute($chunk_spans$);
}
";

            var sb = new StringBuilder();
            var gen_args = "C0";
            var gen_constraints = "where C0 : unmanaged, IComponent";
            var args = "ref C0 c0";
            var chunk_spans = "in Span<C0> chunk0";

            for (int i = 1; i < 16; i++)
            {
                gen_args += $", C{i}";
                gen_constraints += $"\nwhere C{i} : unmanaged, IComponent";
                args += $", ref C{i} c{i}";
                chunk_spans += $", in Span<C{i}> chunk{i}";

                sb.Clear().Append(TEMPLATE)
                    .Replace("$gen_args$", gen_args)
                    .Replace("$args$", args)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$chunk_spans$", chunk_spans);

                context.AddSource($"ForEach{i + 1}.gen.cs", sb.ToString());
            }
        }
    }
}