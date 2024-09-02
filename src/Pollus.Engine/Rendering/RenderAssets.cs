namespace Pollus.Engine.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Engine.Assets;
using Pollus.Utils;

public interface IRenderData : IDisposable
{
}

public interface IRenderDataLoader
{
    int TargetType { get; }

    void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle);
}

public class RenderAssets
{
    Dictionary<int, IRenderDataLoader> loaders = new();
    Dictionary<Handle, IRenderData> renderData = new();

    public RenderAssets Add(Handle handle, IRenderData data)
    {
        renderData.Add(handle, data);
        return this;
    }

    public IRenderData Get(Handle handle)
    {
        return renderData[handle];
    }

    public TRenderData Get<TRenderData>(Handle handle) where TRenderData : IRenderData
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
            throw new InvalidOperationException($"No loader found for type {AssetLookup.GetType(handle.Type)}");
        }

        if (renderData.ContainsKey(handle)) return;
        loader.Prepare(this, gpuContext, assetServer, handle);
    }
}