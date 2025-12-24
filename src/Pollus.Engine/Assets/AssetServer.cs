namespace Pollus.Engine.Assets;

using System.Collections.Concurrent;
using Collections;
using ECS;
using Pollus.Debugging;
using Pollus.Utils;

public class AssetServer : IDisposable
{
    struct AssetLoadState
    {
        public required Handle Handle { get; init; }
        public required AssetPath Path { get; init; }
        public required Task<byte[]> Task { get; init; }
        public required IAssetLoader Loader { get; init; }
    }

    List<IAssetLoader> loaders = new();
    Dictionary<string, int> loaderLookup = new();
    Dictionary<AssetPath, Handle> assetLookup = new();

    ConcurrentQueue<AssetPath> queuedPaths = new();
    ArrayList<AssetLoadState> loadStates = new();

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
        Assets.Init<TAsset>();
        return Load(path);
    }

    public Handle Load(AssetPath path)
    {
        if (assetLookup.TryGetValue(path, out var handle)) return handle;
        if (!AssetIO.Exists(path)) return Handle.Null;
        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx)) return Handle.Null;

        AssetIO.LoadPath(path, out var data);

        var loader = loaders[loaderIdx];
        var loadContext = new LoadContext()
        {
            Path = path,
            FileName = Path.GetFileNameWithoutExtension(path.Path),
            Handle = Assets.GetHandle(path, loader.AssetType),
            AssetServer = this,
        };

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

    public Handle<T> LoadAsync<T>(AssetPath path)
        where T : notnull
    {
        Assets.Init<T>();
        return LoadAsync(path);
    }

    public Handle LoadAsync(AssetPath path)
    {
        if (assetLookup.TryGetValue(path, out var handle)) return handle;
        if (!AssetIO.Exists(path)) return Handle.Null;
        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx)) return Handle.Null;

        var loader = loaders[loaderIdx];
        var loadState = new AssetLoadState()
        {
            Handle = Assets.GetHandle(path, loader.AssetType),
            Path = path,
            Task = AssetIO.LoadPathAsync(path),
            Loader = loader,
        };
        loadStates.Add(loadState);
        return loadState.Handle;
    }

    public void Update()
    {
        for (int i = loadStates.Count - 1; i >= 0; i--)
        {
            ref var loadState = ref loadStates.Get(i);
            if (loadState.Task.IsCompleted is false) continue;
            var data = loadState.Task.Result;

            var loadContext = new LoadContext()
            {
                Path = loadState.Path,
                FileName = Path.GetFileNameWithoutExtension(loadState.Path.Path),
                Handle = loadState.Handle,
                AssetServer = this,
            };

            loadState.Loader.Load(data, ref loadContext);
            if (loadContext.Status == AssetStatus.Loaded)
            {
                var expectedType = TypeLookup.GetType(loadState.Loader.AssetType);
                var asset = loadContext.Asset;
                Guard.IsTrue(asset is not null && asset.GetType() == expectedType, $"AssetServer::Load expected type {expectedType} but got {asset?.GetType()} on path {loadState.Path}");
                Assets.Set(loadState.Handle, asset!);
                assetLookup.TryAdd(loadState.Path, loadState.Handle);
            }

            loadStates.RemoveAt(i);
        }
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