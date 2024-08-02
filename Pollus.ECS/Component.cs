namespace Pollus.ECS;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

public interface IComponent
{

}

public record struct ComponentID(int ID)
{
    public static implicit operator int(ComponentID cid) => cid.ID;
    public static implicit operator ComponentID(int id) => new(id);
}

public static class Component
{
    public readonly struct Info
    {
        public required ComponentID ID { get; init; }
        public required int SizeInBytes { get; init; }
    }

    static class Lookup<C> where C : unmanaged, IComponent
    {
        public static readonly Info Info;

        static Lookup()
        {
            Info = Register<C>();
        }
    }

    static readonly ConcurrentDictionary<Type, ComponentID> componentIDs = new();
    static readonly ConcurrentDictionary<ComponentID, Info> componentInfos = new();

    public static Info Register<T>() where T : unmanaged, IComponent
    {
        var type = typeof(T);
        if (componentIDs.TryGetValue(type, out var id))
            return componentInfos[id];

        var info = new Info
        {
            ID = new ComponentID(componentIDs.Count),
            SizeInBytes = Unsafe.SizeOf<T>(),
        };

        componentIDs[type] = info.ID;
        componentInfos[info.ID] = info;

        return info;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Info GetInfo(ComponentID cid)
    {
        return componentInfos[cid];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Info GetInfo<T>() where T : unmanaged, IComponent
    {
        return Lookup<T>.Info;
    }
}