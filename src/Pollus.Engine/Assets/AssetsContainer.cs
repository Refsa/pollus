namespace Pollus.Engine.Assets;

using System.Diagnostics.CodeAnalysis;
using ECS;
using Utils;
using Core.Assets;

public class AssetsContainer : IDisposable
{
    Dictionary<TypeID, IAssetStorage> assets = new();

    public void Dispose()
    {
        foreach (var asset in assets.Values)
        {
            asset.Dispose();
        }

        assets.Clear();
        GC.SuppressFinalize(this);
    }

    public Assets<TAsset> InitAssets<TAsset>()
        where TAsset : IAsset
    {
        if (!assets.TryGetValue(TypeLookup.ID<TAsset>(), out var storage))
        {
            storage = new Assets<TAsset>();
            assets.Add(TypeLookup.ID<TAsset>(), storage);
        }

        return (Assets<TAsset>)storage;
    }

    public IAssetStorage GetAssets(TypeID typeId)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        return storage;
    }

    public Assets<TAsset> GetAssets<TAsset>()
        where TAsset : IAsset
    {
        return InitAssets<TAsset>();
    }

    public bool TryGetAssets(TypeID typeId, [MaybeNullWhen(false)] out IAssetStorage storage)
    {
        if (assets.TryGetValue(typeId, out var s))
        {
            storage = s;
            return true;
        }

        storage = null;
        return false;
    }

    public bool TryGetAssets<TAsset>([MaybeNullWhen(false)] out Assets<TAsset> storage)
        where TAsset : IAsset
    {
        if (TryGetAssets(TypeLookup.ID<TAsset>(), out var s))
        {
            storage = (Assets<TAsset>)s;
            return true;
        }

        storage = null;
        return false;
    }

    public TAsset? GetAsset<TAsset>(Handle<TAsset> handle)
        where TAsset : IAsset
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        return ((Assets<TAsset>)storage).Get(handle);
    }

    public AssetInfo<TAsset>? GetInfo<TAsset>(Handle<TAsset> handle)
        where TAsset : IAsset
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        return (storage as Assets<TAsset>)?.GetInfo(handle);
    }

    public Handle GetHandle(AssetPath path, TypeID typeId)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        return storage.Initialize(path);
    }

    public Handle<TAsset> GetHandle<TAsset>(AssetPath path)
        where TAsset : IAsset
    {
        return GetHandle(path, TypeLookup.ID<TAsset>());
    }

    public AssetPath? GetPath<TAsset>(Handle<TAsset> handle)
        where TAsset : IAsset
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        return storage.GetPath(handle);
    }

    public AssetPath? GetPath(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        return storage.GetPath(handle);
    }

    public AssetStatus GetStatus<TAsset>(Handle<TAsset> handle)
        where TAsset : IAsset
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        return storage.GetStatus(handle);
    }

    public AssetStatus GetStatus(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        return storage.GetStatus(handle);
    }

    public void SetStatus(Handle handle, AssetStatus status)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        storage.SetStatus(handle, status);
    }

    public Handle<TAsset> AddAsset<TAsset>(TAsset asset, AssetPath? path = null)
        where TAsset : IAsset
    {
        return GetAssets<TAsset>().Add(asset, path);
    }

    public Handle AddAsset(IAsset asset, TypeID typeId, AssetPath? path = null)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        return storage.Add(asset, path);
    }

    public void SetAsset<TAsset>(Handle<TAsset> handle, TAsset asset)
        where TAsset : IAsset
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        storage.Set(handle, asset);
    }

    public void SetAsset(Handle handle, IAsset asset)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        storage.Set(handle, asset);
    }

    public void SetDependencies(Handle handle, HashSet<Handle>? dependencies)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        storage.SetDependencies(handle, dependencies);

        if (dependencies is null) return;
        foreach (var dependency in dependencies)
        {
            if (dependency.IsNull()) throw new InvalidOperationException($"Dependency is null, {dependency}");
            if (!assets.TryGetValue(dependency.Type, out var dependentStorage)) throw new InvalidOperationException($"Asset with handle {dependency} not found");
            dependentStorage.AddDependent(dependency, handle);
        }
    }

    public void NotifyDependants(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        var dependents = storage.GetDependents(handle);

        if (dependents.Count == 0) return;

        var visited = Pool<HashSet<Handle>>.Shared.Rent();
        var queue = Pool<Queue<Handle>>.Shared.Rent();
        try
        {
            visited.Add(handle);
            foreach (var dependent in dependents) queue.Enqueue(dependent);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visited.Add(current);
                if (!TryGetAssets(current.Type, out var dependentStorage)) throw new InvalidOperationException($"Asset storage with type ID {current.Type} not found");
                dependentStorage.AppendEvent(current, AssetEventType.DependenciesChanged);

                foreach (var dependent in dependentStorage.GetDependents(current))
                {
                    if (visited.Contains(dependent)) continue;
                    queue.Enqueue(dependent);
                }
            }
        }
        finally
        {
            visited.Clear();
            queue.Clear();
            Pool<HashSet<Handle>>.Shared.Return(visited);
            Pool<Queue<Handle>>.Shared.Return(queue);
        }
    }

    public bool IsLoaded(Handle handle)
    {
        var visited = Pool<HashSet<Handle>>.Shared.Rent();
        var queue = Pool<Queue<Handle>>.Shared.Rent();
        visited.Add(handle);
        queue.Enqueue(handle);
        try
        {
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!TryGetAssets(current.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {current.Type} not found");
                if (storage.GetStatus(current) != AssetStatus.Loaded) return false;

                var dependencies = storage.GetDependencies(current);
                if (dependencies is null || dependencies.Count == 0) continue;
                foreach (var dependency in dependencies)
                {
                    queue.Enqueue(dependency);
                }
            }
        }
        finally
        {
            visited.Clear();
            queue.Clear();
            Pool<HashSet<Handle>>.Shared.Return(visited);
            Pool<Queue<Handle>>.Shared.Return(queue);
        }

        return true;
    }

    public void FlushEvents(Events events)
    {
        foreach (var asset in assets.Values)
        {
            asset.FlushEvents(events);
        }
    }

    public void ClearEvents()
    {
        foreach (var asset in assets.Values)
        {
            asset.ClearEvents();
        }
    }
}