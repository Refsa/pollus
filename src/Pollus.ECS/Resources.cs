namespace Pollus.ECS;

using Pollus.Utils;

public class Resources : IDisposable
{
    readonly Dictionary<int, object> resources = [];
    readonly List<int> resourceOrder = [];
    bool isDisposed;

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        for (int i = resourceOrder.Count - 1; i >= 0; i--)
        {
            if (resources[resourceOrder[i]] is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        resources.Clear();
        resourceOrder.Clear();
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
        Add(new TResource(), TypeLookup.ID<TResource>());
    }

    public void Add<TResource>(TResource resource)
        where TResource : notnull
    {
        ResourceFetch<TResource>.Register();
        Add(resource, TypeLookup.ID<TResource>());
    }

    public void Add(object obj, int typeId)
    {
        if (!resources.ContainsKey(typeId)) resourceOrder.Add(typeId);
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

    public IEnumerable<TResource> OfType<TResource>()
        where TResource : notnull
    {
        return resources.Values.OfType<TResource>();
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ResourceAttribute : Attribute { }

public class ResourceFetch<TResource> : IFetch<TResource>
    where TResource : notnull
{
    public static void Register()
    {
        Fetch.Register(new ResourceFetch<TResource>(), [typeof(TResource)]);
    }

    public TResource DoFetch(World world, ISystem system)
    {
        return world.Resources.Get<TResource>();
    }
}

public class ResourcesFetch : IFetch<Resources>
{
    public static void Register() => Fetch.Register(new ResourcesFetch(), []);
    public Resources DoFetch(World world, ISystem system) => world.Resources;
}