namespace Pollus.Engine.Assets;

using Core.Serialization;
using ECS;
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

public interface IAssetStorage
{
    TypeID AssetType { get; }
    Handle Add(object asset, AssetPath? path = null);
    void Set(Handle handle, object asset);
    Handle Initialize(AssetPath? path);
    void FlushEvents(Events events);
    AssetStatus GetStatus(Handle handle);
    AssetPath? GetPath(Handle handle);
}

public class Assets<T> : IDisposable, IAssetStorage
    where T : notnull
{
    static readonly TypeID _assetTypeId = TypeLookup.ID<T>();
    static volatile int counter;
    static int NextID => counter++;

    static Assets()
    {
        AssetsFetch<T>.Register();
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<T>());
    }

    public TypeID AssetType => TypeLookup.ID<T>();

    List<AssetInfo<T>> assets = new();
    Dictionary<Handle, int> assetLookup = new();
    Dictionary<AssetPath, Handle> pathLookup = new();

    List<AssetEvent<T>> queuedEvents = new();

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

            queuedEvents.Add(new AssetEvent<T>
            {
                Type = AssetEventType.Added,
                Handle = handle,
            });
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
        queuedEvents.Add(new AssetEvent<T>
        {
            Type = AssetEventType.Added,
            Handle = handle,
        });
        return handle;
    }

    public Handle Add(object asset, AssetPath? path = null)
    {
        if (asset is not T typedAsset) throw new InvalidOperationException($"Asset is not of type {typeof(T)}");
        return Add(typedAsset, path);
    }

    public AssetInfo<T>? GetInfo(Handle handle)
    {
        if (assetLookup.TryGetValue(handle, out var index))
        {
            return assets[index];
        }

        return null;
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

    public AssetPath? GetPath(Handle handle)
    {
        if (assetLookup.TryGetValue(handle, out var index))
        {
            return assets[index].Path;
        }

        return null;
    }

    public void Set(Handle handle, object asset)
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
        assetInfo.Status = AssetStatus.Loaded;

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
            assetInfo.Status = AssetStatus.Unloaded;
            if (assetInfo.Asset is IDisposable disposable)
            {
                disposable.Dispose();
            }

            assets.RemoveAt(index);
            assetLookup.Remove(handle);
            if (assetInfo.Path.HasValue) pathLookup.Remove(assetInfo.Path.Value);

            queuedEvents.Add(new AssetEvent<T>
            {
                Type = AssetEventType.Removed,
                Handle = handle,
            });
        }
    }

    public void FlushEvents(Events events)
    {
        events.InitEvent<AssetEvent<T>>();
        var writer = events.GetWriter<AssetEvent<T>>();

        foreach (var @event in queuedEvents)
        {
            writer.Write(@event);
        }

        queuedEvents.Clear();
    }
}