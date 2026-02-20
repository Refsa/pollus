namespace Pollus.ECS.Generators
{
    using System.Text;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class EntityRefGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(GenerateEntityRefs);
        }

        private void GenerateEntityRefs(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
                @"namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public readonly ref struct EntityRef<$gen_args$>
    $gen_constraints$
{
    static readonly Component.Info[] infos = [$infos$];
    static readonly ComponentID[] cids = [$comp_ids$];

    readonly EntityRef entityRef;
    $component_fields$

    $component_properties$

    public EntityRef(EntityRef entityRef)
    {
        this.entityRef = entityRef;
        $component_init$
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<C>()
        where C : unmanaged, IComponent
    {
        return entityRef.Has<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Added<C>()
        where C : unmanaged, IComponent
    {
        return entityRef.Added<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Removed<C>()
        where C : unmanaged, IComponent
    {
        return entityRef.Removed<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Changed<C>()
        where C : unmanaged, IComponent
    {
        return entityRef.Changed<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetChanged<C>()
        where C : unmanaged, IComponent
    {
        if (!cids.Contains(Component.GetInfo<C>().ID))
        {
            throw new ArgumentException($""{typeof(C)} is not in {typeof(EntityRef<$gen_args$>)}"");
        }

        entityRef.SetChanged<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly C Read<C>()
        where C : unmanaged, IComponent
    {
        return ref entityRef.Get<C>();
    }
}";

            var sb = new StringBuilder();
            var gen_args = "C0";
            var gen_constraints = "where C0 : unmanaged, IComponent";
            var infos = "Component.Register<C0>()";
            var comp_ids = "infos[0].ID";
            var component_fields = "readonly ref C0 component0;";
            var component_properties = "public ref C0 Component0 => ref component0;";
            var component_init = "component0 = ref entityRef.Get<C0>();";

            for (int i = 1; i < 15; i++)
            {
                gen_args += $", C{i}";
                gen_constraints += $"\n    where C{i} : unmanaged, IComponent";
                infos += $", Component.Register<C{i}>()";
                comp_ids += $", infos[{i}].ID";
                component_fields += $"\n    readonly ref C{i} component{i};";
                component_properties += $"\n    public ref C{i} Component{i} => ref component{i};";
                component_init += $"\n        component{i} = ref entityRef.Get<C{i}>();";

                sb.Clear().Append(TEMPLATE)
                    .Replace("$gen_args$", gen_args)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$infos$", infos)
                    .Replace("$comp_ids$", comp_ids)
                    .Replace("$component_fields$", component_fields)
                    .Replace("$component_properties$", component_properties)
                    .Replace("$component_init$", component_init);

                context.AddSource($"EntityRef{i + 1}.gen.cs", sb.ToString());
            }
        }
    }
}
