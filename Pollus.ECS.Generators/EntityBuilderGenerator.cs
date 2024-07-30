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
            context.RegisterPostInitializationOutput(Generate);
        }

        void Generate(IncrementalGeneratorPostInitializationContext context)
        {
            var sb = new StringBuilder();
            for (int i = 2; i < 16; i++)
            {
                sb.Clear();
                Generate(sb, i, i < 15);
                context.AddSource($"EntityBuilder_{i}.cs", sb.ToString());
            }
        }

        static readonly string BASE_TEMPLATE =
@"namespace Pollus.ECS;

public struct EntityBuilder<$gen_args> : IEntityBuilder
$gen_constraints
{
    static readonly ComponentID[] componentIDs = [$component_ids];
    public static ComponentID[] ComponentIDs => componentIDs;
    static readonly ArchetypeID archetypeID = ArchetypeID.Create(componentIDs);
    public static ArchetypeID ArchetypeID => archetypeID;

$fields

    public EntityBuilder($constructor_args)
    {
$set_fields
    }

    public Entity Spawn(World world)
    {
        var (entity, entityInfo, archetype) = world.Archetypes.CreateEntity(this);
        ref var chunk = ref archetype.GetChunk(entityInfo.ChunkIndex);
        
$chunk_set_component

        return entity;
    }

    $with_method
}";

        static readonly string WITH_METHOD_TEMPLATE = @"
    public EntityBuilder<$gen_args, $next_gen_arg> With<$next_gen_arg>(in $next_gen_arg $next_gen_arg_name)
        where $next_gen_arg : unmanaged, IComponent
    {
        return new EntityBuilder<$gen_args, $next_gen_arg>($with_args);
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

            for (int i = 0; i < genArgCount; i++)
            {
                bool isLast = i == genArgCount - 1;

                gen_args.AppendFormat("C{0}{1}", i, isLast ? "" : ", ");
                gen_constraints.AppendFormat("\twhere C{0} : unmanaged, IComponent{1}", i, isLast ? "" : "\n");
                component_ids.AppendFormat("Component.GetInfo<C{0}>().ID{1}", i, isLast ? "" : ", ");
                fields.AppendFormat("\tpublic C{0} Component{0};{1}", i, isLast ? "" : "\n");
                constructor_args.AppendFormat("in C{0} c{0}{1}", i, isLast ? "" : ", ");
                set_fields.AppendFormat("\t\tComponent{0} = c{0};{1}", i, isLast ? "" : "\n");
                with_args.AppendFormat("Component{0}, ", i);
                chunk_set_component.AppendFormat("\t\tchunk.SetComponent(entityInfo.RowIndex, Component{0});{1}", i, isLast ? "" : "\n");
            }

            with_args.Append(next_gen_arg_name);

            sb.Append(BASE_TEMPLATE)
                .Replace("$gen_args", gen_args.ToString())
                .Replace("$gen_constraints", gen_constraints.ToString())
                .Replace("$component_ids", component_ids.ToString())
                .Replace("$fields", fields.ToString())
                .Replace("$constructor_args", constructor_args.ToString())
                .Replace("$set_fields", set_fields.ToString())
                .Replace("$next_gen_arg", next_gen_arg)
                .Replace("$chunk_set_component", chunk_set_component.ToString())
                .Replace("$with_method", genWith is false ? "" : WITH_METHOD_TEMPLATE
                    .Replace("$gen_args", gen_args.ToString())
                    .Replace("$next_gen_arg_name", next_gen_arg_name)
                    .Replace("$next_gen_arg", next_gen_arg)
                    .Replace("$with_args", with_args.ToString())
                );
        }
    }
}