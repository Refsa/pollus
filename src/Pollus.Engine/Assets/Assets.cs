namespace Pollus.Engine.Assets;

using Core.Serialization;
using ECS;
using Pollus.Collections;
using Pollus.Utils;
using Serialization;
using Core.Assets;

public interface IAssetStorage : IDisposable
{
    TypeID AssetType { get; }

    Handle Initialize(AssetPath? path);
    Handle Add(IAsset asset, AssetPath? path = null);
    void Set(Handle handle, IAsset asset);
    void Remove(Handle handle);

    void AppendEvent(Handle handle, AssetEventType type);
    void FlushEvents(Events events);
    void ClearEvents();

    IAssetInfo? GetInfo(Handle handle);

    event Action<Handle, AssetPath?, IAsset> OnAdded;
    event Action<Handle, AssetPath?, IAsset> OnModified;
    event Action<Handle, AssetPath?> OnRemoved;
}

public class Assets<T> : IAssetStorage
    where T : IAsset
{
    static readonly TypeID _assetTypeId = TypeLookup.ID<T>();
    static volatile int counter;
    static int NextID => Interlocked.Increment(ref counter) - 1;

    static Assets()
    {
        AssetsFetch<T>.Register();
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<T>());
    }

    List<AssetInfo<T>> assets = new();
    Dictionary<Handle, int> assetLookup = new();
    Dictionary<AssetPath, Handle> pathLookup = new();

    ArrayList<AssetEvent<T>> queuedEvents = new();

    public event Action<Handle, AssetPath?, IAsset>? OnAdded;
    public event Action<Handle, AssetPath?, IAsset>? OnModified;
    public event Action<Handle, AssetPath?>? OnRemoved;

    public TypeID AssetType => _assetTypeId;
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
                if (info.Asset is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                info.Asset = asset;
                info.Status = AssetStatus.Loaded;
                info.Dependencies = [.. asset.Dependencies];
                info.LastModified = DateTime.UtcNow;

                OnModified?.Invoke(handle, path, asset);
            }

            queuedEvents.Add(new AssetEvent<T>
            {
                Type = AssetEventType.Changed,
                Handle = handle,
            });
            return handle;
        }

        handle = new Handle<T>(NextID);
        if (path.HasValue) pathLookup.Add(path.Value, handle);
        assetLookup.Add(handle, assets.Count);

        var newInfo = new AssetInfo<T>
        {
            Handle = handle,
            Status = AssetStatus.Added,
            Asset = asset,
            Path = path,
            Dependencies = [.. asset.Dependencies],
            LastModified = DateTime.UtcNow,
        };

        assets.Add(newInfo);
        OnAdded?.Invoke(handle, path, asset);

        queuedEvents.Add(new AssetEvent<T>
        {
            Type = AssetEventType.Added,
            Handle = handle,
        });
        return handle;
    }

    public Handle Add(IAsset asset, AssetPath? path = null)
    {
        if (asset is not T typedAsset) throw new InvalidOperationException($"Asset is not of type {typeof(T)}");
        return Add(typedAsset, path);
    }

    public void Set(Handle handle, IAsset asset)
    {
        if (asset is not T typedAsset) throw new InvalidOperationException($"Asset is not of type {typeof(T)}");
        Set(handle, typedAsset);
    }

    public void Set(Handle handle, T asset)
    {
        if (!assetLookup.TryGetValue(handle, out var index))
        {
            throw new InvalidOperationException($"Asset with handle {handle} not found");
        }

        var assetInfo = assets[index];
        assetInfo.Asset = asset;
        assetInfo.Status = AssetStatus.Added;
        assetInfo.LastModified = DateTime.UtcNow;
        assetInfo.Dependencies = [.. asset.Dependencies];

        OnModified?.Invoke(handle, assetInfo.Path, asset);

        queuedEvents.Add(new AssetEvent<T>
        {
            Type = AssetEventType.Changed,
            Handle = handle,
        });
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

            for (int i = index; i < assets.Count; i++)
            {
                assetLookup[assets[i].Handle] = i;
            }

            OnRemoved?.Invoke(handle, assetInfo.Path);

            queuedEvents.Add(new AssetEvent<T>
            {
                Type = AssetEventType.Deleted,
                Handle = handle,
            });
        }
    }

    public T? Get(Handle handle)
    {
        if (assetLookup.TryGetValue(handle, out var index))
        {
            return assets[index].Asset;
        }

        return default;
    }

    public T? Get(AssetPath path)
    {
        if (pathLookup.TryGetValue(path, out var handle))
        {
            return Get(handle);
        }

        return default;
    }

    IAssetInfo? IAssetStorage.GetInfo(Handle handle)
    {
        if (assetLookup.TryGetValue(handle, out var index))
        {
            return assets[index];
        }

        return null;
    }

    public AssetInfo<T>? GetInfo(Handle handle)
    {
        return ((IAssetStorage)this).GetInfo(handle) as AssetInfo<T>;
    }

    public void AppendEvent(Handle handle, AssetEventType type)
    {
        if (!assetLookup.ContainsKey(handle)) throw new InvalidOperationException($"Asset with handle {handle} not found");
        queuedEvents.Add(new AssetEvent<T>
        {
            Type = type,
            Handle = handle,
        });
    }

    public void FlushEvents(Events events)
    {
        events.InitEvent<AssetEvent<T>>();
        var writer = events.GetWriter<AssetEvent<T>>();
        writer.Append(queuedEvents.AsSpan());
        queuedEvents.Clear();
    }

    public void ClearEvents()
    {
        queuedEvents.Clear();
    }
}
