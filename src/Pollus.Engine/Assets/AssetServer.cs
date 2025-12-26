namespace Pollus.Engine.Assets;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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
        public required Task<Result<byte[], AssetIO.Error>> Task { get; init; }
        public required IAssetLoader Loader { get; init; }
    }

    List<IAssetLoader> loaders = new();
    Dictionary<string, int> loaderLookup = new();
    Dictionary<AssetPath, Handle> assetLookup = new();

    ConcurrentDictionary<AssetPath, DateTime> queuedPaths = new();
    ArrayList<AssetLoadState> loadStates = new();

    bool isDisposed;

    public AssetIO AssetIO { get; }
    public AssetsContainer Assets { get; }

    public bool FileWatchEnabled => AssetIO.FileWatchEnabled;

    public AssetServer(AssetIO assetIO)
    {
        AssetIO = assetIO;
        Assets = new();
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
            if (queuedPaths.TryAdd(obj, DateTime.UtcNow) is false)
            {
                queuedPaths[obj] = DateTime.UtcNow;
            }
        }
    }

    public void EnableFileWatch()
    {
        AssetIO.OnAssetChanged += OnAssetChanged;
        AssetIO.EnableFileWatch();
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

    public void InitAssets<TAsset>()
        where TAsset : notnull
    {
        Assets.InitAssets<TAsset>();
    }

    public Assets<TAsset> GetAssets<TAsset>()
        where TAsset : notnull
    {
        return Assets.GetAssets<TAsset>();
    }
    
    public IAssetStorage GetAssets(TypeID typeId)
    {
        return Assets.GetAssets(typeId);
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
        if (queuedPaths.TryAdd(path, DateTime.UtcNow) is false)
        {
            queuedPaths[path] = DateTime.UtcNow;
        }

        return handle;
    }

    public Handle<TAsset> Load<TAsset>(AssetPath path, bool reload = false)
        where TAsset : notnull
    {
        Assets.InitAssets<TAsset>();
        return Load(path);
    }

    public Handle Load(AssetPath path, bool reload = false)
    {
        if (!reload && assetLookup.TryGetValue(path, out var handle)) return handle;
        if (!AssetIO.Exists(path)) return Handle.Null;
        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx)) return Handle.Null;

        var loadResult = AssetIO.LoadPath(path);
        if (loadResult.IsErr())
        {
            Log.Error((FormattableString)$"AssetServer::Load failed to load asset {path}:\n{loadResult.ToErr()}");
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

        loader.Load(loadResult.Unwrap(), ref loadContext);
        if (loadContext.Status == AssetStatus.Loaded)
        {
            var expectedType = TypeLookup.GetType(loader.AssetType);
            var asset = loadContext.Asset;
            Guard.IsTrue(asset is not null && asset.GetType() == expectedType, $"AssetServer::Load expected type {expectedType} but got {asset?.GetType()} on path {path}");
            handle = Assets.AddAsset(asset!, loader.AssetType, path);
            Assets.SetDependencies(handle, loadContext.Dependencies);
            assetLookup.TryAdd(path, handle);
            Assets.NotifyDependants(handle);
            return handle;
        }

        return Handle.Null;
    }

    public Handle<T> LoadAsync<T>(AssetPath path)
        where T : notnull
    {
        Assets.InitAssets<T>();
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
        var count = loadStates.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            ref var loadState = ref loadStates.Get(i);
            if (loadState.Task.IsCompleted is false) continue;

            var loadResult = loadState.Task.Result;
            if (loadResult.IsErr())
            {
                Log.Error($"AssetServer::Update failed to load asset {loadState.Path}:\n{loadResult.ToErr()}");
                loadStates.RemoveAt(i);
                continue;
            }

            var loadContext = new LoadContext()
            {
                Path = loadState.Path,
                FileName = Path.GetFileNameWithoutExtension(loadState.Path.Path),
                Handle = loadState.Handle,
                AssetServer = this,
            };

            loadState.Loader.Load(loadResult.Unwrap(), ref loadContext);
            if (loadContext.Status == AssetStatus.Loaded)
            {
                var expectedType = TypeLookup.GetType(loadState.Loader.AssetType);
                var asset = loadContext.Asset;
                Guard.IsTrue(asset is not null && asset.GetType() == expectedType, $"AssetServer::Load expected type {expectedType} but got {asset?.GetType()} on path {loadState.Path}");
                Assets.SetAsset(loadState.Handle, asset!);
                assetLookup.TryAdd(loadState.Path, loadState.Handle);
            }

            loadStates.RemoveAt(i);
        }
    }

    public void FlushLoading()
    {
        while (loadStates.Count > 0)
        {
            Update();
        }
    }

    public void FlushQueue()
    {
        foreach (var kvp in queuedPaths)
        {
            if (kvp.Value > DateTime.UtcNow.AddMilliseconds(-300)) continue;
            queuedPaths.TryRemove(kvp.Key, out _);
            _ = Load(kvp.Key, true);
            Log.Info((FormattableString)$"AssetServer::FlushQueue {kvp.Key}");
        }
    }
}