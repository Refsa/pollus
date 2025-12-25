namespace Pollus.Engine.Assets;

using ECS;
using Utils;

public class AssetsContainer : IDisposable
{
    bool isDisposed;
    readonly Dictionary<TypeID, IAssetStorage> assets = [];

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        foreach (var asset in assets.Values)
        {
            if (asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        assets.Clear();
    }

    public void Init<T>()
        where T : notnull
    {
        if (assets.ContainsKey(TypeLookup.ID<T>()))
        {
            return;
        }

        assets.Add(TypeLookup.ID<T>(), new Assets<T>());
    }

    public bool TryGetAssets<T>(out Assets<T> container)
        where T : notnull
    {
        if (assets.TryGetValue(TypeLookup.ID<T>(), out var containerObject))
        {
            container = (Assets<T>)containerObject;
            return true;
        }

        container = null!;
        return false;
    }

    public Assets<T> GetAssets<T>()
        where T : notnull
    {
        if (!TryGetAssets<T>(out var container))
        {
            var id = TypeLookup.ID<T>();
            container = new Assets<T>();
            assets.Add(id, container);
        }

        return container;
    }

    public bool TryGetAssets(TypeID typeId, out IAssetStorage storage)
    {
        if (assets.TryGetValue(typeId, out var asset))
        {
            storage = asset;
            return true;
        }

        storage = null!;
        return false;
    }

    public Handle<T> AddAsset<T>(T asset, AssetPath? path = null)
        where T : notnull
    {
        return GetAssets<T>().Add(asset, path);
    }

    public Handle AddAsset(object asset, TypeID typeId, AssetPath? path = null)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        return storage.Add(asset, path);
    }

    public void SetAsset<T>(Handle<T> handle, T asset)
        where T : notnull
    {
        GetAssets<T>().SetAsset(handle, asset);
    }

    public void SetAsset(Handle handle, object asset)
    {
        if (!TryGetAssets(handle.Type, out var storage))
        {
            throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        }

        storage.SetAsset(handle, asset);
    }

    public void SetDependencies(Handle handle, List<Handle>? dependencies)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        storage.SetDependencies(handle, dependencies);
    }

    public Handle<T> GetHandle<T>(AssetPath path)
        where T : notnull
    {
        return GetAssets<T>().Initialize(path);
    }

    public Handle GetHandle(AssetPath path, TypeID typeId)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        return storage.Initialize(path);
    }

    public T? Get<T>(Handle<T> handle)
        where T : notnull
    {
        if (TryGetAssets<T>(out var container))
        {
            return container.Get(handle);
        }

        return default;
    }

    public AssetStatus GetStatus<T>(Handle handle)
        where T : notnull
    {
        if (TryGetAssets<T>(out var container))
        {
            return container.GetStatus(handle);
        }

        return AssetStatus.Unknown;
    }

    public AssetPath? GetPath<T>(Handle<T> handle)
        where T : notnull
    {
        if (TryGetAssets<T>(out var container))
        {
            return container.GetPath(handle);
        }

        return null;
    }

    public void FlushEvents(Events events)
    {
        foreach (var asset in assets.Values)
        {
            asset.FlushEvents(events);
        }
    }
}