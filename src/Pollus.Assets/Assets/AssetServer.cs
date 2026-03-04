namespace Pollus.Assets;

using System.Collections.Concurrent;
using Pollus.Collections;
using Pollus.Core.Assets;
using Pollus.Debugging;
using Pollus.Utils;

public class AssetServer : IDisposable
{
    enum LoadPhase
    {
        Loading,
        WaitingForDeps,
    }

    struct AssetLoadState
    {
        public required Handle Handle { get; init; }
        public required AssetPath Path { get; init; }
        public required IAssetLoader Loader { get; init; }

        public Task<Result<byte[], AssetIO.Error>>? Task { get; init; }
        public byte[]? Data { get; init; }

        public LoadPhase Phase { get; set; }
        public object? State { get; set; }
        public HashSet<Handle>? Dependencies { get; set; }
    }

    List<IAssetLoader> loaders = new();
    Dictionary<string, int> loaderLookup = new();

    ConcurrentDictionary<AssetPath, DateTime> queuedPaths = new();
    ArrayList<AssetLoadState> loadStates = new();
    CancellationTokenSource loadCts = new();

    bool isDisposed;

    public AssetIO AssetIO { get; }
    public AssetsContainer Assets { get; }

    public bool FileWatchEnabled => AssetIO.FileWatchEnabled;
    public int PendingLoads => loadStates.Count;

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

        loadCts.Cancel();
        loadCts.Dispose();
        loadStates.Clear();
    }

    public void EnableFileWatch()
    {
        AssetIO.OnAssetChanged += OnAssetChanged;
        AssetIO.EnableFileWatch();
    }

    void OnAssetChanged(AssetPath obj)
    {
        Queue(obj);
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

    public Assets<TAsset> InitAssets<TAsset>()
        where TAsset : IAsset
    {
        return Assets.InitAssets<TAsset>();
    }

    public Assets<TAsset> GetAssets<TAsset>()
        where TAsset : IAsset
    {
        return Assets.GetAssets<TAsset>();
    }

    public IAssetStorage GetAssets(TypeID typeId)
    {
        return Assets.GetAssets(typeId);
    }

    public Handle<TAsset> Queue<TAsset>(AssetPath path)
        where TAsset : IAsset
    {
        Assets.InitAssets<TAsset>();
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
        where TAsset : IAsset
    {
        Assets.InitAssets<TAsset>();
        return Load(path, reload);
    }

    public Handle Load(AssetPath path, bool reload = false)
    {
        if (!reload && Assets.TryGetHandle(path, out var handle) && Assets.IsLoaded(handle)) return handle;
        if (!AssetIO.Exists(path))
        {
            Log.Error((FormattableString)$"AssetServer::Load asset {path} does not exist");
            return Handle.Null;
        }

        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx)) return Handle.Null;

        var loadResult = AssetIO.LoadPath(path);
        if (loadResult.TryErr(out var error))
        {
            Log.Error((FormattableString)$"AssetServer::Load failed to load asset {path}:\n{error}");
            return Handle.Null;
        }

        var loader = loaders[loaderIdx];
        var loadState = new AssetLoadState()
        {
            Handle = Assets.GetHandle(path, loader.AssetType),
            Path = path,
            Loader = loader,
            Data = loadResult.Unwrap(),
        };

        if (!ProcessLoadState(ref loadState) &&
            loadState is { Phase: LoadPhase.WaitingForDeps, Dependencies.Count: > 0 })
        {
            foreach (var dep in loadState.Dependencies)
            {
                if (!Assets.IsLoaded(dep) && Assets.GetInfo(dep)?.Path is { } depPath)
                    Load(depPath);
            }
            ProcessLoadState(ref loadState);
        }

        return Assets.IsLoaded(loadState.Handle) ? loadState.Handle : Handle.Null;
    }

    public Handle<TAsset> LoadAsync<TAsset>(AssetPath path, bool reload = false)
        where TAsset : IAsset
    {
        Assets.InitAssets<TAsset>();
        return LoadAsync(path, reload);
    }

    public Handle LoadAsync(AssetPath path, bool reload = false)
    {
        if (!reload && Assets.TryGetHandle(path, out var handle)) return handle;
        if (!AssetIO.Exists(path))
        {
            Log.Error((FormattableString)$"AssetServer::LoadAsync asset {path} does not exist");
            return Handle.Null;
        }

        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx)) return Handle.Null;

        var loader = loaders[loaderIdx];
        var loadState = new AssetLoadState()
        {
            Handle = Assets.GetHandle(path, loader.AssetType),
            Path = path,
            Loader = loader,
            Task = AssetIO.LoadPathAsync(path, loadCts.Token),
        };
        loadStates.Add(loadState);
        return loadState.Handle;
    }

    public void Update()
    {
        for (int i = loadStates.Count - 1; i >= 0; i--)
        {
            if (ProcessLoadState(ref loadStates.Get(i)))
                loadStates.RemoveAt(i);
        }
    }

    bool ProcessLoadState(ref AssetLoadState loadState)
    {
        var info = Assets.GetInfo(loadState.Handle);
        if (info is null or { Status: AssetStatus.Failed })
            return true;

        if (loadState.Phase is LoadPhase.Loading)
        {
            byte[] data;
            if (loadState.Data is not null)
            {
                data = loadState.Data;
            }
            else if (loadState.Task is { } task)
            {
                if (!task.IsCompleted) return false;

                var loadResult = task.Result;
                if (loadResult.IsErr())
                {
                    Log.Error($"AssetServer::Update failed to load asset {loadState.Path}:\n{loadResult.ToErr()}");
                    Assets.SetFailed(loadState.Handle);
                    return true;
                }
                data = loadResult.Unwrap();
            }
            else
            {
                return true;
            }

            var loadContext = CreateLoadContext(ref loadState);
            loadState.Loader.Load(data, ref loadContext);

            if (loadContext.Status == AssetLoadStatus.Loaded)
            {
                FinalizeAsset(ref loadContext);
                return true;
            }

            if (loadContext.Status != AssetLoadStatus.Preprocess)
            {
                Log.Warn($"AssetServer::Update loader for {loadState.Path} produced no asset");
                return true;
            }

            loadState.State = loadContext.State;
            loadState.Dependencies = loadContext.Dependencies;
            loadState.Phase = LoadPhase.WaitingForDeps;
        }

        if (loadState.Phase is LoadPhase.WaitingForDeps)
        {
            bool depsReady = AreDependenciesReady(loadState.Dependencies, loadState.Handle, out bool failed);
            if (failed) return true;
            if (!depsReady) return false;

            var resolveContext = CreateLoadContext(ref loadState);
            resolveContext.State = loadState.State;
            resolveContext.Dependencies = loadState.Dependencies;
            loadState.Loader.Resolve(ref resolveContext);

            if (resolveContext.Status == AssetLoadStatus.Loaded)
            {
                FinalizeAsset(ref resolveContext);
            }
        }

        return true;
    }

    LoadContext CreateLoadContext(ref AssetLoadState loadState)
    {
        return new LoadContext()
        {
            Path = loadState.Path,
            FileName = Path.GetFileNameWithoutExtension(loadState.Path.Path),
            Handle = loadState.Handle,
            AssetServer = this,
            Loader = loadState.Loader,
        };
    }

    bool AreDependenciesReady(HashSet<Handle>? dependencies, Handle owner, out bool failed)
    {
        failed = false;
        if (dependencies is not { Count: > 0 }) return true;

        foreach (var dep in dependencies)
        {
            if (Assets.IsFailed(dep))
            {
                failed = true;
                Assets.SetFailed(owner);
                return false;
            }
            if (!Assets.IsLoaded(dep)) return false;
        }
        return true;
    }

    void FinalizeAsset(ref LoadContext loadContext)
    {
        var expectedType = TypeLookup.GetType(loadContext.Loader.AssetType);
        var asset = loadContext.Asset;
        Guard.IsNotNull(asset, $"AssetServer::FinalizeAsset expected asset on path {loadContext.Path}");
        Guard.IsTrue(asset.GetType() == expectedType, $"AssetServer::FinalizeAsset expected type {expectedType} but got {asset.GetType()} on path {loadContext.Path}");

        if (loadContext.Dependencies is { Count: > 0 })
        {
            asset.Dependencies.UnionWith(loadContext.Dependencies);
        }

        Assets.AddAsset(asset, loadContext.Loader.AssetType, loadContext.Path);
    }

    public void FlushLoading()
    {
        var timeout = DateTime.UtcNow.AddSeconds(10);
        while (loadStates.Count > 0 && DateTime.UtcNow < timeout)
        {
            Update();
            Thread.Yield();
        }

        if (loadStates.Count > 0)
        {
            Log.Error("AssetServer::FlushLoading timed out");
        }
    }

    public void FlushQueue()
    {
        if (queuedPaths.IsEmpty) return;
        foreach (var kvp in queuedPaths)
        {
            if (kvp.Value > DateTime.UtcNow.AddMilliseconds(-300)) continue;
            queuedPaths.TryRemove(kvp.Key, out _);
            _ = LoadAsync(kvp.Key, true);
            Log.Info((FormattableString)$"AssetServer::FlushQueue {kvp.Key}");
        }
    }
}
