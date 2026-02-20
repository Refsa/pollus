namespace Pollus.ECS.Generators
{
    using System.Text;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class ViewGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(GenerateViews);
        }

        private void GenerateViews(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
                @"namespace Pollus.ECS;

using System.Runtime.CompilerServices;

/// <inheritdoc cref=""IView"" />
public readonly struct View<$gen_args$> : IView, IViewCreate<View<$gen_args$>>
    $gen_constraints$
{
    static readonly Component.Info[] infos = [$infos$];
    static readonly ComponentID[] cids = [$comp_ids$];
    public static Component.Info[] Infos => infos;

    public static View<$gen_args$> Create(World world) => new(world);

    static View()
    {
        ViewFetch<View<$gen_args$>>.Register();
    }

    readonly Query query;

    public View(World world)
    {
        query = new Query(world);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        return query.Has<C>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Added<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        return query.Added<C>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Removed<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        return query.Removed<C>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Changed<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        return query.Changed<C>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetChanged<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!cids.Contains(Component.GetInfo<C>().ID))
            throw new ArgumentException($""{typeof(C).Name} is not in {this.GetType().Name}"", nameof(entity));

        query.SetChanged<C>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyChanged<C>()
        where C : unmanaged, IComponent
    {
        return query.AnyChanged<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyAdded<C>()
        where C : unmanaged, IComponent
    {
        return query.AnyAdded<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly C Read<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        return ref query.Get<C>(entity);
    }

    /// <summary>
    /// C has to be part of View generic args.
    /// </summary>
    /// <typeparam name=""C""></typeparam>
    /// <param name=""entity""></param>
    /// <returns>nullref if C is not part of View generic parameters</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C Get<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!cids.Contains(Component.GetInfo<C>().ID))
            throw new ArgumentException($""{typeof(C).Name} is not in {this.GetType().Name}"", nameof(entity));

        return ref query.Get<C>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C GetTracked<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!cids.Contains(Component.GetInfo<C>().ID))
            throw new ArgumentException($""{typeof(C).Name} is not in {this.GetType().Name}"", nameof(entity));

        return ref query.GetTracked<C>(entity);
    }

    public EntityRef<$gen_args$> GetEntity(in Entity entity)
    {
        return new(query.GetEntity(entity));
    }
}";

            var sb = new StringBuilder();
            var gen_args = "C0";
            var gen_constraints = "where C0 : unmanaged, IComponent";
            var infos = "Component.Register<C0>()";
            var comp_ids = "infos[0].ID";

            for (int i = 1; i < 15; i++)
            {
                gen_args += $", C{i}";
                gen_constraints += $"\n    where C{i} : unmanaged, IComponent";
                infos += $", Component.Register<C{i}>()";
                comp_ids += $", infos[{i}].ID";

                sb.Clear().Append(TEMPLATE)
                    .Replace("$gen_args$", gen_args)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$infos$", infos)
                    .Replace("$comp_ids$", comp_ids);

                context.AddSource($"View{i + 1}.gen.cs", sb.ToString());
            }
        }
    }
}
