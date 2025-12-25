namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public interface IRenderDataLoader
{
    TypeID TargetType { get; }
    void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle);
    void Unload(RenderAssets renderAssets, Handle handle);
}

public class RenderAssetInfo
{
    public required Handle Handle { get; set; }
    public required RenderAssetStatus Status { get; set; }
    public required object Asset { get; set; }
    public required DateTime LastModified { get; set; }

    public HashSet<Handle> Dependencies { get; set; } = [];
    public HashSet<Handle> AssetDependencies { get; set; } = [];
}

public enum RenderAssetStatus
{
    Initialized = 0,
    Loaded,
    Unloaded,
}

public class RenderAssets : IRenderAssets, IDisposable
{
    static volatile int counter;
    static int NextID => Interlocked.Increment(ref counter);

    readonly Dictionary<TypeID, IRenderDataLoader> loaders = [];
    readonly Dictionary<Handle, RenderAssetInfo> renderData = [];

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var data in renderData.Values)
        {
            if (data.Asset is IDisposable disposable) disposable.Dispose();
        }

        renderData.Clear();
    }

    public bool Has(Handle handle)
    {
        return renderData.ContainsKey(handle);
    }

    public RenderAssetInfo? GetInfo(Handle handle)
    {
        return renderData.GetValueOrDefault(handle);
    }

    IRenderAssets IRenderAssets.Add<T>(Handle handle, T data)
    {
        return Add(handle, data);
    }

    public RenderAssets Add<T>(Handle handle, T data)
        where T : notnull
    {
        renderData.Add(handle, new()
        {
            Handle = handle,
            Asset = data,
            LastModified = DateTime.UtcNow,
            Status = RenderAssetStatus.Loaded,
        });
        return this;
    }

    public Handle<T> Add<T>(T data)
        where T : notnull
    {
        var handle = new Handle<T>(NextID);
        renderData.Add(handle, new()
        {
            Handle = handle,
            Asset = data,
            LastModified = DateTime.UtcNow,
            Status = RenderAssetStatus.Loaded,
        });
        return handle;
    }

    public object Get(Handle handle)
    {
        return renderData[handle];
    }

    public TRenderData Get<TRenderData>(Handle handle) where TRenderData : notnull
    {
        return (TRenderData)renderData[handle].Asset;
    }

    public TRenderData Get<TRenderData>(Handle<TRenderData> handle) where TRenderData : notnull
    {
        return (TRenderData)renderData[handle].Asset;
    }

    public bool TryGet<TRenderData>(Handle handle, out TRenderData data) where TRenderData : notnull
    {
        if (renderData.TryGetValue(handle, out var value))
        {
            data = (TRenderData)value.Asset;
            return true;
        }

        data = default!;
        return false;
    }

    public RenderAssets AddLoader<TLoader>(TLoader loader) where TLoader : IRenderDataLoader
    {
        loaders.TryAdd(loader.TargetType, loader);
        return this;
    }

    public RenderAssets AddLoader<TLoader>() where TLoader : IRenderDataLoader, new()
    {
        var loader = new TLoader();
        loaders.TryAdd(loader.TargetType, loader);
        return this;
    }

    public void Prepare(IWGPUContext gpuContext, AssetServer assetServer, Handle handle, bool reload = false)
    {
        if (!loaders.TryGetValue(handle.Type, out var loader))
        {
            throw new InvalidOperationException($"No loader found for type {TypeLookup.GetType(handle.Type)}");
        }

        var exists = renderData.ContainsKey(handle);
        if (exists && !reload) return;
        if (exists && reload) Unload(handle);
        loader.Prepare(this, gpuContext, assetServer, handle);
    }

    public void Unload(Handle handle)
    {
        if (renderData.TryGetValue(handle, out var data))
        {
            if (loaders.TryGetValue(handle.Type, out var loader)) loader.Unload(this, handle);
            else if (data is IDisposable disposable) disposable.Dispose();
            renderData.Remove(handle);
        }
    }
}