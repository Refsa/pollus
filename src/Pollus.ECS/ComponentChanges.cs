namespace Pollus.ECS;

using System;

public enum ComponentFlags
{
    Added,
    Changed,
    Removed,
}

public class ComponentChanges
{
    struct Change
    {
        public ComponentID ComponentID;
        public ulong RemovedVersion;
    }

    class ChangeList
    {
        Change[] changes = new Change[4];
        int count;

        public ReadOnlySpan<Change> Changes => changes.AsSpan(0, count);

        public void SetFlag(in ComponentID cid, ulong version)
        {
            ref var change = ref GetOrCreate(cid);
            change.RemovedVersion = version;
        }

        public bool HasFlag(in ComponentID cid, ulong version)
        {
            ref var change = ref GetOrCreate(cid);
            return version - change.RemovedVersion <= 1;
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
    SparseSet<ChangeList> changes = new(32);

    public void Tick(ulong version)
    {
        this.version = version;
        foreach (var change in changes)
        {
            change.Clear();
        }
    }

    public void SetRemoved<C>(Entity entity)
        where C : unmanaged, IComponent
    {
        SetRemoved(entity, Component.GetInfo<C>().ID);
    }

    public void SetRemoved(Entity entity, ComponentID cid)
    {
        if (!changes.Contains(entity.ID))
        {
            changes.Add(entity.ID, new ChangeList());
        }

        var list = changes.Get(entity.ID);
        list.SetFlag(cid, version);
    }
    
    public bool WasRemoved<C>(Entity entity)
        where C : unmanaged, IComponent
    {
        return WasRemoved(entity, Component.GetInfo<C>().ID);
    }

    public bool WasRemoved(Entity entity, ComponentID cid)
    {
        return changes.Contains(entity.ID) && changes.Get(entity.ID).HasFlag(cid, version);
    }
}