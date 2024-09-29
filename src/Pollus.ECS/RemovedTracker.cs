namespace Pollus.ECS;

using Pollus.Collections;

public enum ComponentFlags
{
    Added,
    Changed,
    Removed,
}

public class RemovedTracker
{
    ulong version = 0;
    List<IRemovedTracker> trackers = new();
    Dictionary<int, int> trackerLookup = new();

    public void Tick(ulong version)
    {
        this.version = version;
        foreach (var tracker in new ListEnumerable<IRemovedTracker>(trackers))
        {
            tracker.Tick(version);
        }
    }

    public RemovedTracker<C> GetTracker<C>()
        where C : unmanaged, IComponent
    {
        if (!trackerLookup.TryGetValue(Component.GetInfo<C>().ID, out var index))
        {
            index = trackers.Count;
            trackerLookup.Add(Component.GetInfo<C>().ID, index);
            var tracker = new RemovedTracker<C>();
            tracker.Tick(version);
            trackers.Add(tracker);
        }

        return (RemovedTracker<C>)trackers[index];
    }

    public void SetRemoved<C>(Entity entity, in C component)
        where C : unmanaged, IComponent
    {
        GetTracker<C>().SetRemoved(entity, in component);
    }

    public bool WasRemoved<C>(Entity entity)
        where C : unmanaged, IComponent
    {
        return GetTracker<C>().WasRemoved(entity);
    }

    public ref C GetRemoved<C>(Entity entity)
        where C : unmanaged, IComponent
    {
        return ref GetTracker<C>().GetRemoved(entity);
    }
}

public interface IRemovedTracker
{
    void Tick(ulong version);
    bool WasRemoved(Entity entity);
}

public class RemovedTracker<C> : IRemovedTracker
    where C : unmanaged, IComponent
{
    static RemovedTracker() => RemoveTrackerFetch<C>.Register();

    struct Removed
    {
        public int Entity;
        public C Component;
        public ulong Version;
    }

    SparseSet<Removed> tracker = new(32);
    ulong version = 0;

    public void Tick(ulong version)
    {
        this.version = version;
        foreach (var removed in tracker)
        {
            if (version - removed.Version <= 1) continue;
            tracker.Remove(removed.Entity);
        }
    }

    public void SetRemoved(Entity entity, in C component)
    {
        if (!tracker.Contains(entity.ID))
        {
            tracker.Add(entity.ID, new Removed { Entity = entity.ID, Component = component, Version = version });
            return;
        }

        var removed = tracker.Get(entity.ID);
        removed.Component = component;
        removed.Version = version;
    }

    public bool WasRemoved(Entity entity)
    {
        if (!tracker.Contains(entity.ID)) return false;
        return (version - tracker.Get(entity.ID).Version) <= 1;
    }

    public ref C GetRemoved(Entity entity)
    {
        return ref tracker.Get(entity.ID).Component;
    }
}

public class RemoveTrackerFetch<C> : IFetch<RemovedTracker<C>>
    where C : unmanaged, IComponent
{
    public static void Register()
    {
        Fetch.Register(new RemoveTrackerFetch<C>(), []);
    }

    public RemovedTracker<C> DoFetch(World world, ISystem system)
    {
        return world.Store.Removed.GetTracker<C>();
    }
}