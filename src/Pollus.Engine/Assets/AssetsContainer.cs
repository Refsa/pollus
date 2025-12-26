namespace Pollus.Engine.Assets;

using System.Diagnostics.CodeAnalysis;
using ECS;
using Utils;

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
        where TAsset : notnull
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
        where TAsset : notnull
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
        where TAsset : notnull
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
        where TAsset : notnull
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        return ((Assets<TAsset>)storage).Get(handle);
    }

    public Handle GetHandle(AssetPath path, TypeID typeId)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        return storage.Initialize(path);
    }

    public Handle<TAsset> GetHandle<TAsset>(AssetPath path)
        where TAsset : notnull
    {
        return GetHandle(path, TypeLookup.ID<TAsset>());
    }

    public AssetPath? GetPath<TAsset>(Handle<TAsset> handle)
        where TAsset : notnull
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
        where TAsset : notnull
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        return storage.GetStatus(handle);
    }

    public AssetStatus GetStatus(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        return storage.GetStatus(handle);
    }

    public Handle AddAsset<TAsset>(TAsset asset, AssetPath? path = null)
        where TAsset : notnull
    {
        return GetAssets<TAsset>().Add(asset, path);
    }

    public Handle AddAsset(object asset, TypeID typeId, AssetPath? path = null)
    {
        if (!TryGetAssets(typeId, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {typeId} not found");
        return storage.Add(asset, path);
    }

    public void SetAsset<TAsset>(Handle<TAsset> handle, TAsset asset)
        where TAsset : notnull
    {
        if (!TryGetAssets(TypeLookup.ID<TAsset>(), out var storage)) throw new InvalidOperationException($"Asset storage with type ID {TypeLookup.ID<TAsset>()} not found");
        storage.SetAsset(handle, asset);
    }

    public void SetAsset(Handle handle, object asset)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        storage.SetAsset(handle, asset);
    }

    public void SetDependencies(Handle handle, HashSet<Handle>? dependencies)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        storage.SetDependencies(handle, dependencies);
    }

    public void NotifyDependants(Handle handle)
    {
        if (!TryGetAssets(handle.Type, out var storage)) throw new InvalidOperationException($"Asset storage with type ID {handle.Type} not found");
        storage.NotifyDependants(handle);
    }

    public void FlushEvents(Events events)
    {
        foreach (var asset in assets.Values)
        {
            asset.FlushEvents(events);
        }
    }
}