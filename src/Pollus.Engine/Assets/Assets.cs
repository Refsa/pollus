namespace Pollus.Engine.Assets;

public record struct Handle(int AssetType, int ID)
{
    public override int GetHashCode()
    {
        return HashCode.Combine(AssetType, ID);
    }
}

public record struct Handle<T> where T : notnull
{
    public int AssetType { get; }
    public int ID { get; }

    public Handle(int id)
    {
        AssetType = AssetLookup.ID<T>();
        ID = id;
    }

    public static implicit operator Handle(Handle<T> handle) => new(handle.AssetType, handle.ID);
    public static implicit operator Handle<T>(Handle handle) => new(handle.ID);

    public override int GetHashCode()
    {
        return HashCode.Combine(AssetType, ID);
    }
}

public enum AssetStatus
{
    Failed = -1,
    Unknown = 0,
    Initialized,
    Unloaded,
    Loading,
    Loaded,
}

public class AssetInfo<T>
{
    public Handle Handle { get; set; }
    public AssetStatus Status { get; set; }
    public AssetPath? Path { get; set; }
    public T? Asset { get; set; }
}

public class Assets<T> : IDisposable
    where T : notnull
{
    static int _assetTypeId = AssetLookup.ID<T>();
    static volatile int counter;
    static int NextID => counter++;

    Dictionary<Handle, AssetInfo<T>> assets = new();

    public IEnumerable<Handle> Handles => assets.Keys;

    public void Dispose()
    {
        foreach (var asset in assets.Values)
        {
            if (asset.Asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        assets.Clear();
    }

    public Handle Initialize(AssetPath? path)
    {
        var handle = new Handle(_assetTypeId, NextID);
        assets.Add(handle, new AssetInfo<T>
        {
            Handle = handle,
            Status = AssetStatus.Initialized,
            Path = path,
        });
        return handle;
    }

    public Handle<T> Add(T asset, AssetPath? path = null)
    {
        var handle = new Handle<T>(NextID);
        assets.Add(handle, new AssetInfo<T>
        {
            Handle = handle,
            Status = AssetStatus.Loaded,
            Asset = asset,
            Path = path,
        });
        return handle;
    }

    public T? Get(Handle handle)
    {
        if (assets.TryGetValue(handle, out var assetHandle))
        {
            return assetHandle.Asset;
        }

        return default;
    }

    public AssetStatus GetStatus(Handle handle)
    {
        if (assets.TryGetValue(handle, out var assetHandle))
        {
            return assetHandle.Status;
        }

        return AssetStatus.Unknown;
    }

    public void Unload(Handle handle)
    {
        if (assets.TryGetValue(handle, out var assetHandle))
        {
            assetHandle.Status = AssetStatus.Unloaded;
            if (assetHandle.Asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
            assetHandle.Asset = default;
        }
    }

    public void Remove(Handle handle)
    {
        if (assets.TryGetValue(handle, out var assetHandle))
        {
            if (assetHandle.Asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
            assets.Remove(handle);
        }
    }
}

public class Assets : IDisposable
{
    Dictionary<int, object> assets = new();

    public void Dispose()
    {
        foreach (var asset in assets.Values)
        {
            if (asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        assets.Clear();
    }

    public bool TryGetAssets<T>(out Assets<T> container)
        where T : notnull
    {
        if (assets.TryGetValue(AssetLookup.ID<T>(), out var containerObject))
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
            var id = AssetLookup.ID<T>();
            container = new Assets<T>();
            assets.Add(id, container);
        }
        return container;
    }

    public Handle<T> Add<T>(T asset, AssetPath? path = null)
        where T : notnull
    {
        return GetAssets<T>().Add(asset, path);
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
}