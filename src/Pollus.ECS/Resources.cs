namespace Pollus.ECS;

using Pollus.Utils;

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
        ResourceFetch<TResource>.Register();
        return TypeLookup.ID<TResource>();
    }

    public void Add<TResource>()
        where TResource : notnull, new()
    {
        ResourceFetch<TResource>.Register();
        resources[TypeLookup.ID<TResource>()] = new TResource();
    }

    public void Add<TResource>(TResource resource)
        where TResource : notnull
    {
        ResourceFetch<TResource>.Register();
        resources[TypeLookup.ID<TResource>()] = resource;
    }

    public void Add(object obj, int typeId)
    {
        resources[typeId] = obj;
    }

    public TResource Get<TResource>()
        where TResource : notnull
    {
        if (!resources.TryGetValue(TypeLookup.ID<TResource>(), out var resource))
        {
            throw new KeyNotFoundException($"Resource of type {typeof(TResource).Name} not found.");
        }

        return (TResource)resource;
    }

    public bool TryGet<TResource>(out TResource resource)
        where TResource : notnull
    {
        if (resources.TryGetValue(TypeLookup.ID<TResource>(), out var res))
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
        return resources.ContainsKey(TypeLookup.ID<TResource>());
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ResourceAttribute : Attribute { }

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