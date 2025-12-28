namespace Pollus.Engine.Assets;

using System.Diagnostics.CodeAnalysis;
using ECS;
using Utils;
using Core.Assets;

public class AssetsContainer : IDisposable
{
    Dictionary<TypeID, IAssetStorage> assets = new();
    Dictionary<AssetPath, Handle> assetLookup = new();

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
            var s = new Assets<TAsset>();
            s.OnAdded += OnAssetAddedOrModified;
            s.OnModified += OnAssetAddedOrModified;
            s.OnRemoved += OnAssetRemoved;
            storage = s;
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

    public IAssetInfo? GetInfo(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        return storage.GetInfo(handle);
    }

    public AssetInfo<TAsset>? GetInfo<TAsset>(Handle<TAsset> handle)
        where TAsset : IAsset
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        return (storage as Assets<TAsset>)?.GetInfo(handle);
    }

    public bool TryGetHandle(AssetPath path, out Handle handle)
    {
        if (assetLookup.TryGetValue(path, out var h))
        {
            handle = h;
            return true;
        }

        handle = Handle.Null;
        return false;
    }

    public Handle GetHandle(AssetPath? path, TypeID typeId)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        return storage.Initialize(path);
    }

    public Handle<TAsset> GetHandle<TAsset>(AssetPath path)
        where TAsset : IAsset
    {
        return GetHandle(path, TypeLookup.ID<TAsset>());
    }

    public Handle<TAsset> AddAsset<TAsset>(TAsset asset, AssetPath? path = null)
        where TAsset : IAsset
    {
        var handle = GetAssets<TAsset>().Add(asset, path);
        if (path.HasValue) assetLookup.TryAdd(path.Value, handle);
        return handle;
    }

    public Handle AddAsset(IAsset asset, TypeID typeId, AssetPath? path = null)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        var handle = storage.Add(asset, path);
        if (path.HasValue) assetLookup.TryAdd(path.Value, handle);
        return handle;
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

    public void RemoveAsset(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        var info = storage.GetInfo(handle);
        if (info?.Path is { } path)
        {
            assetLookup.Remove(path);
        }
        OnAssetRemoved(handle, info);
        storage.Remove(handle);
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
        if (!TryGetAssets(handle.Type, out var storage)) return;
        var info = storage.GetInfo(handle);
        if (info == null || info.Dependents.Count == 0) return;

        var visited = Pool<HashSet<Handle>>.Shared.Rent();
        var queue = Pool<Queue<Handle>>.Shared.Rent();
        try
        {
            visited.Add(handle);
            foreach (var dependent in info.Dependents) queue.Enqueue(dependent);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current)) continue;

                if (ValidateStatus(current))
                {
                    if (!TryGetAssets(current.Type, out var dependentStorage)) continue;
                    var subInfo = dependentStorage.GetInfo(current);
                    if (subInfo != null)
                    {
                        foreach (var dependent in subInfo.Dependents)
                        {
                            if (visited.Contains(dependent)) continue;
                            queue.Enqueue(dependent);
                        }
                    }
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
        if (!TryGetAssets(handle.Type, out var storage)) return false;
        var info = storage.GetInfo(handle);
        return info?.Status == AssetStatus.Loaded;
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

    void OnAssetAddedOrModified(Handle handle, IAsset asset)
    {
        if (asset.Dependencies is { Count: > 0 })
        {
            foreach (var dep in asset.Dependencies)
            {
                if (TryGetAssets(dep.Type, out var depStorage))
                {
                    depStorage.AddDependent(dep, handle);
                }
            }
        }

        ValidateStatus(handle);
        NotifyDependants(handle);
    }

    void OnAssetRemoved(Handle handle, IAssetInfo? info)
    {
        if (info == null) return;

        if (info.Dependents is { Count: > 0 })
        {
            foreach (var dependent in info.Dependents)
            {
                if (TryGetAssets(dependent.Type, out var dependentStorage))
                {
                    var dependentInfo = dependentStorage.GetInfo(dependent);
                    dependentInfo?.Dependencies?.Remove(handle);
                }
            }
        }

        if (info.Dependencies is { Count: > 0 })
        {
            foreach (var dep in info.Dependencies)
            {
                if (TryGetAssets(dep.Type, out var depStorage))
                {
                    var depInfo = depStorage.GetInfo(dep);
                    depInfo?.Dependents.Remove(handle);
                }
            }
        }
    }

    void OnAssetRemoved(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) return;
        var info = storage.GetInfo(handle);
        OnAssetRemoved(handle, info);
    }

    bool ValidateStatus(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) return false;
        var info = storage.GetInfo(handle);
        if (info == null) return false;

        var newStatus = AssetStatus.Loaded;
        if (info.Dependencies is { Count: > 0 })
        {
            foreach (var dep in info.Dependencies)
            {
                if (!IsLoaded(dep))
                {
                    newStatus = AssetStatus.WaitingForDependency;
                    break;
                }
            }
        }

        if (info.Status != newStatus)
        {
            if (newStatus == AssetStatus.Loaded)
            {
                storage.AppendEvent(handle, AssetEventType.Loaded);
            }

            info.Status = newStatus;
            return true;
        }

        return false;
    }
}
