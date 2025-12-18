namespace Pollus.Engine.Assets;

using System.Runtime.CompilerServices;
using Core.Serialization;
using Pollus.Collections;
using Pollus.Utils;
using Serialization;

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
    static int _assetTypeId = TypeLookup.ID<T>();
    static volatile int counter;
    static int NextID => Interlocked.Increment(ref counter);

    static Assets()
    {
        AssetsFetch<T>.Register();
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<T>());
    }

    List<AssetInfo<T>> assets = new();
    Dictionary<Handle, int> assetLookup = new();
    Dictionary<AssetPath, Handle> pathLookup = new();

    public ListEnumerable<AssetInfo<T>> AssetInfos => new(assets);

    public void Dispose()
    {
        foreach (var asset in assets)
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
        if (path.HasValue && pathLookup.TryGetValue(path.Value, out var handle))
        {
            return handle;
        }

        handle = new Handle(_assetTypeId, NextID);
        if (path.HasValue) pathLookup.Add(path.Value, handle);
        assetLookup.Add(handle, assets.Count);
        assets.Add(new AssetInfo<T>
        {
            Handle = handle,
            Status = AssetStatus.Initialized,
            Path = path,
        });
        return handle;
    }

    public Handle<T> Add(T asset, AssetPath? path = null)
    {
        if (path.HasValue && pathLookup.TryGetValue(path.Value, out var handle))
        {
            if (assetLookup.TryGetValue(handle, out var index))
            {
                var info = assets[index];
                info.Asset = asset;
                info.Status = AssetStatus.Loaded;
            }

            return handle;
        }

        handle = new Handle<T>(NextID);
        if (path.HasValue) pathLookup.Add(path.Value, handle);
        assetLookup.Add(handle, assets.Count);
        assets.Add(new AssetInfo<T>
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
        if (assetLookup.TryGetValue(handle, out var index))
        {
            return assets[index].Asset;
        }

        return default;
    }

    public AssetStatus GetStatus(Handle handle)
    {
        if (assetLookup.TryGetValue(handle, out var index))
        {
            return assets[index].Status;
        }

        return AssetStatus.Unknown;
    }

    public void Unload(Handle handle)
    {
        if (assetLookup.TryGetValue(handle, out var index))
        {
            var assetInfo = assets[index];
            assetInfo.Status = AssetStatus.Unloaded;
            if (assetInfo.Asset is IDisposable disposable)
            {
                disposable.Dispose();
            }

            assetInfo.Asset = default;
        }
    }

    public void Remove(Handle handle)
    {
        if (assetLookup.TryGetValue(handle, out var index))
        {
            var assetInfo = assets[index];
            if (assetInfo.Asset is IDisposable disposable)
            {
                disposable.Dispose();
            }

            assets.RemoveAt(index);
            assetLookup.Remove(handle);
            if (assetInfo.Path.HasValue) pathLookup.Remove(assetInfo.Path.Value);
        }
    }
}

public class Assets : IDisposable
{
    bool isDisposed;
    readonly Dictionary<int, object> assets = [];

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

    public Handle<T> Add<T>(T asset, AssetPath? path = null)
        where T : notnull
    {
        return GetAssets<T>().Add(asset, path);
    }

    public Handle<T> GetHandle<T>(AssetPath path)
        where T : notnull
    {
        return GetAssets<T>().Initialize(path);
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