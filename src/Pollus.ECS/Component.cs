namespace Pollus.ECS;

using System;
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

[Flags]
public enum ComponentFlags : byte
{
    None = 0,
    Added = 1 << 0,
    Changed = 1 << 1,
    Removed = 1 << 2,
}

public static class Component
{
    public readonly struct Info
    {
        public required ComponentID ID { get; init; }
        public required int SizeInBytes { get; init; }
        public required Type Type { get; init; }
    }

    static class Lookup<C> where C : unmanaged, IComponent
    {
        public static readonly Info Info;

        static Lookup()
        {
            Info = Register<C>();
        }
    }

    static readonly ConcurrentDictionary<Type, Info> componentIDs = new();
    static readonly ConcurrentDictionary<ComponentID, Info> componentInfos = new();

    public static Info Register<T>() where T : unmanaged, IComponent
    {
        var type = typeof(T);
        if (componentIDs.TryGetValue(type, out var info))
            return info;

        info = new Info
        {
            ID = new ComponentID(componentIDs.Count),
            SizeInBytes = Unsafe.SizeOf<T>(),
            Type = type
        };

        if (new T() is IComponentWrapper)
        {
            var wrappedInfo = ComponentWrapper<T>.Info;
            info = new Info
            {
                ID = wrappedInfo.ID,
                Type = wrappedInfo.Type,
                SizeInBytes = Unsafe.SizeOf<T>(),
            };
        }

        componentIDs[type] = info;
        componentInfos[info.ID] = info;

        return info;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Info GetInfo(Type type)
    {
        return componentIDs[type];
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

public class ComponentChanges
{
    struct Change
    {
        public ComponentID ComponentID;
        public ComponentFlags Flags;
    }

    class ChangeList
    {
        Change[] changes = new Change[1];
        int count;

        public ReadOnlySpan<Change> Changes => changes.AsSpan(0, count);

        public void Add(ComponentID id, ComponentFlags flags)
        {
            if (count >= changes.Length)
            {
                var newChanges = new Change[changes.Length * 2];
                Changes.CopyTo(newChanges);
                changes = newChanges;
            }

            changes[count++] = new Change { ComponentID = id, Flags = flags };
        }

        public void Clear()
        {
            count = 0;
        }
    }

    Stack<ChangeList> pool = new();
    Dictionary<Entity, ChangeList> changes = new();

    public void Clear()
    {
        foreach (var list in changes.Values)
        {
            list.Clear();
            pool.Push(list);
        }
        changes.Clear();
    }

    public void AddChange<C>(Entity entity, ComponentFlags flags)
        where C : unmanaged, IComponent
    {
        AddChange(entity, Component.GetInfo<C>().ID, flags);
    }

    public void AddChange(Entity entity, ComponentID id, ComponentFlags flags)
    {
        if (!changes.TryGetValue(entity, out var list))
        {
            list = pool.Count > 0 ? pool.Pop() : new();
            changes[entity] = list;
        }
        list.Add(id, flags);
    }

    public bool HasChange<C>(in Entity entity, ComponentFlags flags)
        where C : unmanaged, IComponent
    {
        return HasChange(entity, Component.GetInfo<C>().ID, flags);
    }

    public bool HasChange(in Entity entity, ComponentID id, ComponentFlags flags)
    {
        if (!changes.TryGetValue(entity, out var list))
            return false;

        foreach (var change in list.Changes)
        {
            if (change.ComponentID == id && change.Flags.HasFlag(flags))
                return true;
        }

        return false;
    }
}