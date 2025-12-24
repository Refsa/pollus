namespace Pollus.Engine.Assets;

using System.Collections.Concurrent;
using ECS;
using Pollus.Debugging;
using Pollus.Utils;

public class AssetServer : IDisposable
{
    List<IAssetLoader> loaders = new();
    Dictionary<string, int> loaderLookup = new();
    Dictionary<AssetPath, Handle> assetLookup = new();
    ConcurrentQueue<AssetPath> queuedPaths = new();

    bool isDisposed;

    public AssetIO AssetIO { get; }
    public AssetsContainer Assets { get; } = new();

    public AssetServer(AssetIO assetIO)
    {
        AssetIO = assetIO;
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);
        Assets.Dispose();
    }

    void OnAssetChanged(AssetPath obj)
    {
        if (assetLookup.ContainsKey(obj))
        {
            queuedPaths.Enqueue(obj);
        }
    }

    public void Watch()
    {
        AssetIO.OnAssetChanged += OnAssetChanged;
        AssetIO.Watch();
    }

    public AssetServer AddLoader<TLoader>() where TLoader : IAssetLoader, new()
    {
        var idx = loaders.Count;
        TLoader loader = new();
        loaders.Add(loader);
        foreach (var ext in loader.Extensions)
        {
            loaderLookup.Add(ext, idx);
        }

        return this;
    }

    public AssetServer AddLoader<TLoader>(TLoader loader) where TLoader : IAssetLoader
    {
        var idx = loaders.Count;
        loaders.Add(loader);
        foreach (var ext in loader.Extensions)
        {
            loaderLookup.Add(ext, idx);
        }

        return this;
    }

    public void InitAsset<TAsset>()
        where TAsset : notnull
    {
        Assets.Init<TAsset>();
    }

    public Assets<TAsset> GetAssets<TAsset>()
        where TAsset : notnull
    {
        return Assets.GetAssets<TAsset>();
    }

    public Handle<TAsset> Queue<TAsset>(AssetPath path)
        where TAsset : notnull
    {
        return Queue(path);
    }

    public Handle Queue(AssetPath path)
    {
        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx))
        {
            return Handle.Null;
        }

        var loader = loaders[loaderIdx];
        if (!Assets.TryGetAssets(loader.AssetType, out var storage))
        {
            return Handle.Null;
        }

        var handle = storage.Initialize(path);
        queuedPaths.Enqueue(path);
        return handle;
    }

    public Handle<TAsset> Load<TAsset>(AssetPath path)
        where TAsset : notnull
    {
        return Load(path);
    }

    public Handle Load(AssetPath path)
    {
        if (assetLookup.TryGetValue(path, out var handle))
        {
            return handle;
        }

        if (!AssetIO.Exists(path))
        {
            return Handle.Null;
        }

        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx))
        {
            return Handle.Null;
        }

        var loader = loaders[loaderIdx];
        var loadContext = new LoadContext()
        {
            Path = path,
            FileName = Path.GetFileNameWithoutExtension(path.Path),
            Handle = Assets.GetHandle(path, loader.AssetType),
            AssetServer = this,
        };

        AssetIO.LoadPath(path, out var data);
        loader.Load(data, ref loadContext);
        if (loadContext.Status == AssetStatus.Loaded)
        {
            var expectedType = TypeLookup.GetType(loader.AssetType);
            var asset = loadContext.Asset;
            Guard.IsTrue(asset is not null && asset.GetType() == expectedType, $"AssetServer::Load expected type {expectedType} but got {asset?.GetType()} on path {path}");
            handle = Assets.Add(asset!, loader.AssetType, path);
            assetLookup.TryAdd(path, handle);
            return handle;
        }

        return Handle.Null;
    }

    public void FlushQueue()
    {
        while (queuedPaths.TryDequeue(out var path))
        {
            Load(path);
        }
    }

    public void FlushEvents(Events events)
    {
        Assets.FlushEvents(events);
    }
}