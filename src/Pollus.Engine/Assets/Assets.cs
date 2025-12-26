namespace Pollus.Engine.Assets;

using System.Runtime.InteropServices;
using Core.Serialization;
using ECS;
using Pollus.Collections;
using Pollus.Utils;
using Serialization;

public enum AssetStatus
{
    Failed = -1,
    Unknown = 0,
    Unloaded = 1,
    Initialized,
    Loading,
    WaitingForDependency,
    Loaded,
}

public interface IAssetInfo
{
    public Handle Handle { get; set; }
    public AssetStatus Status { get; set; }
    public AssetPath? Path { get; set; }

    public HashSet<Handle>? Dependencies { get; set; }
    public HashSet<Handle> Dependents { get; set; }
    public DateTime LastModified { get; set; }
}

public class AssetInfo<T> : IAssetInfo
{
    public Handle Handle { get; set; }
    public AssetStatus Status { get; set; }
    public AssetPath? Path { get; set; }
    public T? Asset { get; set; }

    public HashSet<Handle>? Dependencies { get; set; }
    public HashSet<Handle> Dependents { get; set; } = [];
    public DateTime LastModified { get; set; }
}

public interface IAssetStorage : IDisposable
{
    TypeID AssetType { get; }

    Handle Initialize(AssetPath? path);
    Handle Add(object asset, AssetPath? path = null);
    void SetAsset(Handle handle, object asset);

    void AppendEvent(Handle handle, AssetEventType type);
    void FlushEvents(Events events);
    void ClearEvents();

    AssetStatus GetStatus(Handle handle);
    void SetStatus(Handle handle, AssetStatus status);

    AssetPath? GetPath(Handle handle);

    void AddDependent(Handle handle, Handle dependent);
    IReadOnlySet<Handle> GetDependents(Handle handle);
    void SetDependencies(Handle handle, HashSet<Handle>? dependencies);
    IReadOnlySet<Handle>? GetDependencies(Handle handle);
}

public class Assets<T> : IAssetStorage
    where T : notnull
{
    static readonly TypeID _assetTypeId = TypeLookup.ID<T>();
    static volatile int counter;
    static int NextID => Interlocked.Exchange(ref counter, counter + 1);

    static Assets()
    {
        AssetsFetch<T>.Register();
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<T>());
    }

    List<AssetInfo<T>> assets = new();
    Dictionary<Handle, int> assetLookup = new();
    Dictionary<AssetPath, Handle> pathLookup = new();

    List<AssetEvent<T>> queuedEvents = new();

    public TypeID AssetType => TypeLookup.ID<T>();
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
                info.LastModified = DateTime.UtcNow;
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
        assets.Add(new AssetInfo<T>
        {
            Handle = handle,
            Status = AssetStatus.Loaded,
            Asset = asset,
            Path = path,
            LastModified = DateTime.UtcNow,
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

    public void SetStatus(Handle handle, AssetStatus status)
    {
        if (!assetLookup.TryGetValue(handle, out var index)) throw new InvalidOperationException($"Asset with handle {handle} not found");
        assets[index].Status = status;
    }

    public AssetPath? GetPath(Handle handle)
    {
        if (assetLookup.TryGetValue(handle, out var index))
        {
            return assets[index].Path;
        }

        return null;
    }

    public void SetDependencies(Handle handle, HashSet<Handle>? dependencies)
    {
        if (!assetLookup.TryGetValue(handle, out var index)) throw new InvalidOperationException($"Asset with handle {handle} not found");
        assets[index].Dependencies = dependencies;
    }

    public IReadOnlySet<Handle>? GetDependencies(Handle handle)
    {
        if (!assetLookup.TryGetValue(handle, out var index)) throw new InvalidOperationException($"Asset with handle {handle} not found");
        return assets[index].Dependencies;
    }

    public void AddDependent(Handle handle, Handle dependent)
    {
        if (!assetLookup.TryGetValue(handle, out var index)) throw new InvalidOperationException($"Asset with handle {handle} not found");
        assets[index].Dependents.Add(dependent);
    }

    public IReadOnlySet<Handle> GetDependents(Handle handle)
    {
        if (!assetLookup.TryGetValue(handle, out var index)) throw new InvalidOperationException($"Asset with handle {handle} not found");
        return assets[index].Dependents;
    }

    public void SetAsset(Handle handle, object asset)
    {
        if (asset is not T typedAsset) throw new InvalidOperationException($"Asset is not of type {typeof(T)}");
        SetAsset(handle, typedAsset);
    }

    public void SetAsset(Handle handle, T asset)
    {
        if (!assetLookup.TryGetValue(handle, out var index))
        {
            throw new InvalidOperationException($"Asset with handle {handle} not found");
        }

        var assetInfo = assets[index];
        assetInfo.Asset = asset;
        assetInfo.Status = AssetStatus.Loaded;
        assetInfo.LastModified = DateTime.UtcNow;

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
                Type = AssetEventType.Unloaded,
                Handle = handle,
            });
        }
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

        foreach (var @event in queuedEvents)
        {
            writer.Write(@event);
        }

        queuedEvents.Clear();
    }

    public void ClearEvents()
    {
        queuedEvents.Clear();
    }
}