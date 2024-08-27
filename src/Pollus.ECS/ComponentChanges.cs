namespace Pollus.ECS;

using System;

[Flags]
public enum ComponentFlags : byte
{
    None = 0,
    Added = 1 << 0,
    Changed = 1 << 1,
    Removed = 1 << 2,
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