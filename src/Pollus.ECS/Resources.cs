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
        resources.Add(Resource.ID<TResource>(), new TResource());
    }

    public void Add<TResource>(TResource resource)
        where TResource : notnull
    {
        resources.Add(Resource.ID<TResource>(), resource);
    }

    public TResource Get<TResource>()
        where TResource : notnull
    {
        return (TResource)resources[Resource.ID<TResource>()];
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
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class ResourceAttribute<TResource> : Attribute
    where TResource : notnull
{
    public ResourceAttribute()
    {
        Fetch.Register(new ResourceFetch<TResource>(), []);
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
        public static int ID = counter++;

        static Type()
        {
            Fetch.Register(new ResourceFetch<T>(), []);
        }
    }

    static volatile int counter = 0;
    public static int ID<T>() where T : notnull => Type<T>.ID;
}