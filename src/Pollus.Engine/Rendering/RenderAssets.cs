namespace Pollus.Engine.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Engine.Assets;
using Pollus.Utils;
using Pollus.Graphics;

public interface IRenderDataLoader
{
    int TargetType { get; }
    void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle);
}

public class RenderAssets : IRenderAssets, IDisposable
{
    static volatile int counter;
    static int NextID => counter++;

    readonly Dictionary<int, IRenderDataLoader> loaders = [];
    readonly Dictionary<Handle, object> renderData = [];

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var data in renderData.Values)
        {
            if (data is IDisposable disposable) disposable.Dispose();
        }
        renderData.Clear();
    }

    IRenderAssets IRenderAssets.Add<T>(Handle handle, T data)
    {
        return Add(handle, data);
    }

    public RenderAssets Add<T>(Handle handle, T data)
        where T : notnull
    {
        renderData.Add(handle, data);
        return this;
    }

    public Handle<T> Add<T>(T data)
        where T : notnull
    {
        var handle = new Handle<T>(NextID);
        renderData.Add(handle, data);
        return handle;
    }

    public object Get(Handle handle)
    {
        return renderData[handle];
    }

    public TRenderData Get<TRenderData>(Handle handle) where TRenderData : notnull
    {
        return (TRenderData)renderData[handle];
    }

    public RenderAssets AddLoader<TLoader>(TLoader loader) where TLoader : IRenderDataLoader
    {
        loaders.Add(loader.TargetType, loader);
        return this;
    }

    public void Prepare(IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        if (!loaders.TryGetValue(handle.Type, out var loader))
        {
            throw new InvalidOperationException($"No loader found for type {TypeLookup.GetType(handle.Type)}");
        }

        if (renderData.ContainsKey(handle)) return;
        loader.Prepare(this, gpuContext, assetServer, handle);
    }

    public void Unload(Handle handle)
    {
        if (renderData.TryGetValue(handle, out var data))
        {
            if (data is IDisposable disposable) disposable.Dispose();
            renderData.Remove(handle);
        }
    }
}