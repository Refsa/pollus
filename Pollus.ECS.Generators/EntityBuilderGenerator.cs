namespace Pollus.ECS.Generators
{
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class EntityBuilderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(GenerateEntityBuilders);
            context.RegisterPostInitializationOutput(GenerateConstructorsOnEntity);
            context.RegisterPostInitializationOutput(GenerateTupleExtensions);
        }

        void GenerateTupleExtensions(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

public static class TupleEntityBuilder
{
    $methods$
}";
            const string METHOD_TEMPLATE =
@"
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static EntityBuilder<$gen_args$> Builder<$gen_args$>(this in ($tuple_params$) tuple)
        $gen_constraints$
    {
        return new($tuple_args$);
    }";

            var sb = new StringBuilder(TEMPLATE);

            var methods = new StringBuilder();
            var gen_args = "C0, ";
            var gen_constraints = "where C0 : unmanaged, IComponent\n";
            var tuple_params = "C0 c0, ";
            var tuple_args = "tuple.c0, ";

            for (int i = 1; i < 15; i++)
            {
                gen_args += $"C{i}";
                gen_constraints += $"where C{i} : unmanaged, IComponent";
                tuple_params += $"C{i} c{i}";
                tuple_args += $"tuple.c{i}";

                methods.AppendLine(METHOD_TEMPLATE
                    .Replace("$gen_args$", gen_args)
                    .Replace("$tuple_params$", tuple_params)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$tuple_args$", tuple_args)
                );

                gen_args += ", ";
                gen_constraints += "\n";
                tuple_params += ", ";
                tuple_args += ", ";
            }

            sb.Replace("$methods$", methods.ToString());
            context.AddSource("TupleEntityBuilder.gen.cs", sb.ToString());
        }

        void GenerateConstructorsOnEntity(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static EntityBuilder<$gen_args$> With<$gen_args$>($parameters$)
        $gen_constraints$
    {
        return new($args$);
    }
";

            var sb = new StringBuilder();
            sb.AppendLine("namespace Pollus.ECS;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("public partial record struct Entity");
            sb.AppendLine("{");

            var gen_args = "";
            var gen_constraints = "";
            var parameters = "";
            var args = "";

            for (int i = 0; i < 15; i++)
            {
                gen_args += $"C{i}";
                gen_constraints += $"where C{i} : unmanaged, IComponent\n";
                parameters += $"in C{i} c{i}";
                args += $"c{i}";

                sb.AppendLine(TEMPLATE
                    .Replace("$gen_args$", gen_args)
                    .Replace("$parameters$", parameters)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$args$", args)
                );

                gen_args += ", ";
                parameters += ", ";
                args += ", ";
            }

            sb.AppendLine("}");
            context.AddSource("Entity.gen.cs", sb.ToString());
        }

        void GenerateEntityBuilders(IncrementalGeneratorPostInitializationContext context)
        {
            var sb = new StringBuilder();
            for (int i = 2; i < 16; i++)
            {
                sb.Clear();
                Generate(sb, i, i < 15);
                context.AddSource($"EntityBuilder_{i}.gen.cs", sb.ToString());
            }
        }

        static readonly string BASE_TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

public struct EntityBuilder<$gen_args$> : IEntityBuilder
$gen_constraints$
{
    static readonly ComponentID[] componentIDs = [$component_ids$];
    public static ComponentID[] ComponentIDs => componentIDs;
    static readonly ArchetypeID archetypeID = ArchetypeID.Create(componentIDs);
    public static ArchetypeID ArchetypeID => archetypeID;

$fields$

    public EntityBuilder($constructor_args$)
    {
$set_fields$
    }

    public static implicit operator EntityBuilder<$gen_args$>(scoped in ($tuple_args$) tuple)
    {
        return new($tuple_set$);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn(World world)
    {
        var entityRef = world.Store.CreateEntity<EntityBuilder<$gen_args$>>();
        ref var chunk = ref entityRef.Archetype.GetChunk(entityRef.ChunkIndex);
        
$chunk_set_component$

        return entityRef.Entity;
    }

    $with_method$
}";

        static readonly string WITH_METHOD_TEMPLATE = @"
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityBuilder<$gen_args$, $next_gen_arg$> With<$next_gen_arg$>(scoped in $next_gen_arg$ $next_gen_arg_name$)
        where $next_gen_arg$ : unmanaged, IComponent
    {
        return new($with_args$);
    }";

        void Generate(StringBuilder sb, int genArgCount, bool genWith)
        {
            var gen_args = new StringBuilder();
            var gen_constraints = new StringBuilder();
            var component_ids = new StringBuilder();
            var fields = new StringBuilder();
            var constructor_args = new StringBuilder();
            var set_fields = new StringBuilder();
            var next_gen_arg = $"C{genArgCount}";
            var next_gen_arg_name = $"c{genArgCount}";
            var with_args = new StringBuilder();
            var chunk_set_component = new StringBuilder();
            var tuple_args = new StringBuilder();
            var tuple_set = new StringBuilder();

            for (int i = 0; i < genArgCount; i++)
            {
                bool isLast = i == genArgCount - 1;

                gen_args.AppendFormat("C{0}{1}", i, isLast ? "" : ", ");
                gen_constraints.AppendFormat("\twhere C{0} : unmanaged, IComponent{1}", i, isLast ? "" : "\n");
                component_ids.AppendFormat("Component.GetInfo<C{0}>().ID{1}", i, isLast ? "" : ", ");
                fields.AppendFormat("\tpublic C{0} Component{0};{1}", i, isLast ? "" : "\n");
                constructor_args.AppendFormat("scoped in C{0} c{0}{1}", i, isLast ? "" : ", ");
                set_fields.AppendFormat("\t\tComponent{0} = c{0};{1}", i, isLast ? "" : "\n");
                with_args.AppendFormat("Component{0}, ", i);
                chunk_set_component.AppendFormat("\t\tchunk.SetComponent(entityRef.RowIndex, Component{0});{1}", i, isLast ? "" : "\n");
                tuple_args.AppendFormat("C{0} c{0}{1}", i, isLast ? "" : ", ");
                tuple_set.AppendFormat("tuple.c{0}{1}", i, isLast ? "" : ", ");
            }

            with_args.Append(next_gen_arg_name);

            sb.Append(BASE_TEMPLATE)
                .Replace("$with_method$", genWith is false ? "" : WITH_METHOD_TEMPLATE)
                .Replace("$gen_args$", gen_args.ToString())
                .Replace("$gen_constraints$", gen_constraints.ToString())
                .Replace("$component_ids$", component_ids.ToString())
                .Replace("$fields$", fields.ToString())
                .Replace("$constructor_args$", constructor_args.ToString())
                .Replace("$set_fields$", set_fields.ToString())
                .Replace("$next_gen_arg$", next_gen_arg)
                .Replace("$chunk_set_component$", chunk_set_component.ToString())
                .Replace("$gen_args$", gen_args.ToString())
                .Replace("$next_gen_arg_name$", next_gen_arg_name)
                .Replace("$next_gen_arg$", next_gen_arg)
                .Replace("$with_args$", with_args.ToString())
                .Replace("$tuple_args$", tuple_args.ToString())
                .Replace("$tuple_set$", tuple_set.ToString())
            ;
        }
    }
}