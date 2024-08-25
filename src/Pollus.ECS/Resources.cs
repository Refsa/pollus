namespace Pollus.ECS;

using Pollus.ECS.Core;

public class Resources : IDisposable
{
    Dictionary<int, object> resources = new();

    public void Dispose()
    {
        foreach (var resource in resources.Values)
        {
            if (resource is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        resources.Clear();
    }

    public int Init<TResource>()
        where TResource : notnull
    {
        return Resource.ID<TResource>();
    }

    public void Add<TResource>()
        where TResource : notnull, new()
    {
        resources[Resource.ID<TResource>()] = new TResource();
    }

    public void Add<TResource>(TResource resource)
        where TResource : notnull
    {
        resources[Resource.ID<TResource>()] = resource;
    }

    public TResource Get<TResource>()
        where TResource : notnull
    {
        if (!resources.TryGetValue(Resource.ID<TResource>(), out var resource))
        {
            throw new KeyNotFoundException($"Resource of type {typeof(TResource).Name} not found.");
        }

        return (TResource)resource;
    }

    public bool TryGet<TResource>(out TResource resource)
        where TResource : notnull
    {
        if (resources.TryGetValue(Resource.ID<TResource>(), out var res))
        {
            resource = (TResource)res;
            return true;
        }

        resource = default!;
        return false;
    }

    public bool Has<TResource>()
        where TResource : notnull
    {
        return resources.ContainsKey(Resource.ID<TResource>());
    }
}

public class ResourceFetch<TResource> : IFetch<TResource>
    where TResource : notnull
{
    public static void Register()
    {
        Fetch.Register(new ResourceFetch<TResource>(), []);
    }

    public TResource DoFetch(World world, ISystem system)
    {
        return world.Resources.Get<TResource>();
    }
}

public static class Resource
{
    static class Type<T>
        where T : notnull
    {
        public static int ID;

        static Type()
        {
            ID = Interlocked.Increment(ref counter);
            Fetch.Register(new ResourceFetch<T>(), []);
        }
    }

    static volatile int counter = 0;
    public static int ID<T>() where T : notnull => Type<T>.ID;
}