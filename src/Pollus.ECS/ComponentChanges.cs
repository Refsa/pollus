namespace Pollus.ECS;

using System;

public enum ComponentFlags : byte
{
    None = 0,
    Added = 1,
    Changed = 2,
    Removed = 3,
}

public class ComponentChanges
{
    struct Change
    {
        public ComponentID ComponentID;
        public ulong AddedVersion;
        public ulong ChangedVersion;
        public ulong RemovedVersion;
    }

    class ChangeList
    {
        Change[] changes = new Change[4];
        int count;

        public ReadOnlySpan<Change> Changes => changes.AsSpan(0, count);

        public void SetFlag(in ComponentID cid, ComponentFlags flag, ulong version)
        {
            ref var change = ref GetOrCreate(cid);
            if (flag == ComponentFlags.Added)
                change.AddedVersion = version;
            else if (flag == ComponentFlags.Changed)
                change.ChangedVersion = version;
            else if (flag == ComponentFlags.Removed)
                change.RemovedVersion = version;
        }

        public bool HasFlag(in ComponentID cid, ComponentFlags flag, ulong version)
        {
            ref var change = ref GetOrCreate(cid);
            if (flag == ComponentFlags.Added)
                return version - change.AddedVersion <= 1;
            else if (flag == ComponentFlags.Changed)
                return version - change.ChangedVersion <= 1;
            else if (flag == ComponentFlags.Removed)
                return version - change.RemovedVersion <= 1;
            return false;
        }

        ref Change GetOrCreate(in ComponentID id)
        {
            for (int i = 0; i < count; i++)
            {
                if (changes[i].ComponentID == id)
                    return ref changes[i];
            }
            
            if (count >= changes.Length) Array.Resize(ref changes, changes.Length * 2);
            ref var change = ref changes[count++];
            change.ComponentID = id;
            return ref change;
        }

        public void Clear()
        {
            count = 0;
        }
    }

    ulong version = 0;
    Dictionary<Entity, ChangeList> changes = new(512);

    public void Tick(ulong version)
    {
        this.version = version;
    }

    public void SetFlag<C>(Entity entity, ComponentFlags flags)
        where C : unmanaged, IComponent
    {
        SetFlag(entity, Component.GetInfo<C>().ID, flags);
    }

    public void SetFlag(Entity entity, ComponentID id, ComponentFlags flags)
    {
        if (!changes.TryGetValue(entity, out var list))
        {
            list = new();
            changes[entity] = list;
        }
        list.SetFlag(id, flags, version);
    }

    public bool HasFlag<C>(in Entity entity, ComponentFlags flags)
        where C : unmanaged, IComponent
    {
        return HasFlag(entity, Component.GetInfo<C>().ID, flags);
    }

    public bool HasFlag(in Entity entity, ComponentID cid, ComponentFlags flags)
    {
        if (changes.TryGetValue(entity, out var list))
        {
            return list.HasFlag(cid, flags, version);
        }
        
        return false;
    }
}