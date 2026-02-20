namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public readonly ref struct EntityRef<C0>
    where C0 : unmanaged, IComponent
{
    static readonly Component.Info[] infos = [Component.Register<C0>()];
    static readonly ComponentID[] cids = [infos[0].ID];

    readonly EntityRef entityRef;
    readonly ref C0 component0;

    public ref C0 Component0 => ref component0;

    public EntityRef(EntityRef entityRef)
    {
        this.entityRef = entityRef;
        component0 = ref entityRef.Get<C0>();
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
            throw new ArgumentException($"{typeof(C)} is not in {typeof(EntityRef<C0>)}");
        }

        entityRef.SetChanged<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly C Read<C>()
        where C : unmanaged, IComponent
    {
        return ref entityRef.Get<C>();
    }
}