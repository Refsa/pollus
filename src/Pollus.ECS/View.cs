namespace Pollus.ECS;

using System.Runtime.CompilerServices;

/// <summary>
/// Allows access to components on entities as long as the component is part of the generic arguments of the View instance.
/// <br/>
/// Differs from Query in that the components is not an exclusive set. <br/>
/// Does not contain iteration methods, just direct entity access <br/>
/// Use this over a naked <see cref="Query"/> as this variant gives type hints to the scheduler.
/// </summary>
public interface IView
{
    static abstract Component.Info[] Infos { get; }
}

public interface IViewCreate<TView>
    where TView : IView
{
    static abstract TView Create(World world);
}

public class ViewFetch<TView> : IFetch<TView>
    where TView : IView, IViewCreate<TView>
{
    public static void Register()
    {
        Fetch.Register(new ViewFetch<TView>(), [.. TView.Infos.Select(e => e.Type), typeof(Query)]);
    }

    public TView DoFetch(World world, ISystem system)
    {
        return TView.Create(world);
    }
}

/// <inheritdoc cref="IView" />
public readonly struct View<C0> : IView, IViewCreate<View<C0>>
    where C0 : unmanaged, IComponent
{
    static readonly Component.Info[] infos = [Component.Register<C0>()];
    static readonly ComponentID[] cids = [infos[0].ID];
    public static Component.Info[] Infos => infos;

    public static View<C0> Create(World world) => new(world);

    static View()
    {
        ViewFetch<View<C0>>.Register();
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
            throw new ArgumentException($"{typeof(C).Name} is not in {this.GetType().Name}", nameof(entity));

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
    /// <typeparam name="C"></typeparam>
    /// <param name="entity"></param>
    /// <returns>nullref if C is not part of View generic parameters</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C Get<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!cids.Contains(Component.GetInfo<C>().ID))
            throw new ArgumentException($"{typeof(C).Name} is not in {this.GetType().Name}", nameof(entity));

        return ref query.Get<C>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C GetTracked<C>(scoped in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!cids.Contains(Component.GetInfo<C>().ID))
            throw new ArgumentException($"{typeof(C).Name} is not in {this.GetType().Name}", nameof(entity));

        return ref query.GetTracked<C>(entity);
    }

    public EntityRef<C0> GetEntity(in Entity entity)
    {
        return new(query.GetEntity(entity));
    }
}
