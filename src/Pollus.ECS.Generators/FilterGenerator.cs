namespace Pollus.ECS.Generators
{
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class FilterGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(GenerateFilters);
        }

        void GenerateFilters(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

$none$
$all$
$any$
$multi$
";

            const string NONE_TEMPLATE =
@"public class None<$gen_args$> : IFilter
    $gen_constraints$
{
    static ComponentID[] componentIDs = [$infos$];
    public static readonly None<$gen_args$> Instance = new();

    public object? this[int index] => null;
    public int Length => $length$;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasAny(componentIDs) is false;
    }
}";

            const string ALL_TEMPLATE =
@"public class All<$gen_args$> : IFilter
    $gen_constraints$
{
    static ComponentID[] componentIDs = [$infos$];
    public static readonly All<$gen_args$> Instance = new();

    public object? this[int index] => null;
    public int Length => $length$;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasAll(componentIDs) is true;
    }
}";

            const string ANY_TEMPLATE =
@"public class Any<$gen_args$> : IFilter
    $gen_constraints$
{
    static ComponentID[] componentIDs = [$infos$];
    public static readonly Any<$gen_args$> Instance = new();

    public object? this[int index] => null;
    public int Length => $length$;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasAny(componentIDs) is true;
    }
}";

            const string MULTI_TEMPLATE =
@"public class Multi<$gen_args$> : IFilter
    $filter_gen_constraints$
{
    public static readonly Multi<$gen_args$> Instance = new();

    $filter_fields$

    public object? this[int index] => null;
    public int Length => 2;

    public bool Filter(Archetype archetype)
    {
        return $filter_conditions$;
    }
}";

            var sb = new StringBuilder();
            var gen_args = "C0";
            var gen_constraints = "where C0 : unmanaged, IComponent";
            var infos = "Component.Register<C0>().ID";
            var filter_fields = "T0 t0 = new();";
            var filter_conditions = "t0.Filter(archetype)";
            var filter_gen_constraints = "where T0 : IFilter, new()";

            for (int i = 1; i < 16; i++)
            {
                gen_args += $", C{i}";
                gen_constraints += $"\nwhere C{i} : unmanaged, IComponent";
                infos += $", Component.Register<C{i}>().ID";
                filter_fields += $"\nT{i} t{i} = new();";
                filter_conditions += $"&& t{i}.Filter(archetype)";
                filter_gen_constraints += $"\nwhere T{i} : IFilter, new()";

                var none = NONE_TEMPLATE
                    .Replace("$gen_args$", gen_args)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$infos$", infos)
                    .Replace("$length$", (i + 1).ToString());

                var all = ALL_TEMPLATE
                    .Replace("$gen_args$", gen_args)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$infos$", infos)
                    .Replace("$length$", (i + 1).ToString());

                var multi = "";
                if (i > 1)
                {
                    multi = MULTI_TEMPLATE
                        .Replace("$gen_args$", gen_args.Replace("C", "T"))
                        .Replace("$filter_gen_constraints$", filter_gen_constraints)
                        .Replace("$filter_fields$", filter_fields)
                        .Replace("$filter_conditions$", filter_conditions + ";");
                }

                var any = "";
                if (i > 1)
                {
                    any = ANY_TEMPLATE
                        .Replace("$gen_args$", gen_args)
                        .Replace("$gen_constraints$", gen_constraints)
                        .Replace("$infos$", infos)
                        .Replace("$length$", (i + 1).ToString());
                }

                sb.Clear().Append(TEMPLATE)
                    .Replace("$none$", none)
                    .Replace("$all$", all)
                    .Replace("$any$", any)
                    .Replace("$multi$", multi)
                    ;

                context.AddSource($"Filter{i + 1}.gen.cs", sb.ToString());
            }
        }
    }
}