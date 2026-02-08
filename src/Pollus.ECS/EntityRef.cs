using System.Runtime.CompilerServices;

namespace Pollus.ECS;

public ref struct EntityRef
{
    readonly int row;
    readonly ref ArchetypeChunk chunk;

    public readonly Entity Entity;

    public EntityRef(in Entity entity, int row, ref ArchetypeChunk chunk)
    {
        Entity = entity;
        this.row = row;
        this.chunk = ref chunk;
    }

    public readonly bool Has<C>()
        where C : unmanaged, IComponent
    {
        return chunk.HasComponent<C>();
    }

    public readonly ref C Get<C>()
        where C : unmanaged, IComponent
    {
        return ref chunk.GetComponent<C>(row);
    }

    public readonly ref C TryGet<C>(out bool exists)
        where C : unmanaged, IComponent
    {
        if (Has<C>())
        {
            exists = true;
            return ref Get<C>();
        }
        exists = false;
        return ref Unsafe.NullRef<C>();
    }

    public readonly void SetChanged<C>()
        where C : unmanaged, IComponent
    {
        chunk.SetFlag<C>(row, ComponentFlags.Changed);
    }

    public readonly bool Added<C>()
        where C : unmanaged, IComponent
    {
        return chunk.HasFlag<C>(row, ComponentFlags.Added);
    }

    public readonly bool Changed<C>()
        where C : unmanaged, IComponent
    {
        return chunk.HasFlag<C>(row, ComponentFlags.Changed);
    }

    public readonly bool Removed<C>()
        where C : unmanaged, IComponent
    {
        return chunk.HasFlag<C>(row, ComponentFlags.Removed);
    }
}