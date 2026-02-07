namespace Pollus.ECS;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public sealed class RequiredAttribute<C> : Attribute
    where C : unmanaged, IComponent
{
    public readonly Type ComponentType = typeof(C);
    public string? Constructor { get; }

    public RequiredAttribute()
    {
    }

    public RequiredAttribute(string constructor)
    {
        Constructor = constructor;
    }
}

public readonly struct ComponentDefaultData(ComponentID cid, byte[] data)
{
    public readonly ComponentID CID = cid;
    public readonly byte[] Data = data;
}

public static class RequiredComponents
{
    public interface IContainer
    {
        Dictionary<ComponentID, byte[]> Defaults { get; }
        ComponentID[] ComponentIDs { get; }

        void SetComponents(scoped ref ArchetypeChunk chunk, int rowIndex, ReadOnlySpan<ComponentID> skip);
    }

    class Container<C> : IContainer
        where C : unmanaged, IComponent
    {
        public Dictionary<ComponentID, byte[]> Defaults { get; }
        public ComponentID[] ComponentIDs { get; }

        public Container()
        {
            Defaults = [];
            C.CollectRequired(Defaults);
            ComponentIDs = [.. Defaults.Keys];
        }

        public void SetComponents(scoped ref ArchetypeChunk chunk, int rowIndex, ReadOnlySpan<ComponentID> skip)
        {
            foreach (scoped ref readonly var cid in ComponentIDs.AsSpan())
            {
                if (skip.Contains(cid)) continue;
                chunk.SetComponent(rowIndex, cid, Defaults[cid]);
            }
        }
    }

    static readonly Dictionary<ComponentID, IContainer> containers = new();

    public static IContainer Init<C>()
        where C : unmanaged, IComponent
    {
        if (containers.TryGetValue(Component.GetInfo<C>().ID, out var container)) return container;

        container = new Container<C>();
        containers[Component.GetInfo<C>().ID] = container;
        return container;
    }

    public static IContainer Get<C>()
        where C : unmanaged, IComponent
    {
        if (containers.TryGetValue(Component.GetInfo<C>().ID, out var container))
        {
            return container;
        }

        return Init<C>();
    }

    public static IContainer Get(ComponentID id)
    {
        if (!containers.TryGetValue(id, out var container))
            throw new KeyNotFoundException($"Component {id} is not registered");
        return container;
    }

    public static bool TryGet(ComponentID id, out IContainer container)
    {
        return containers.TryGetValue(id, out container!);
    }
}
