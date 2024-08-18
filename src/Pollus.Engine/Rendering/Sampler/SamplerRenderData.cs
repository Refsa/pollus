namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public class SamplerRenderData : IRenderData
{
    public required GPUSampler Sampler { get; init; }

    public void Dispose()
    {
        Sampler.Dispose();
    }
}

public class SamplerRenderDataLoader : IRenderDataLoader
{
    public int TargetType => AssetLookup.ID<SamplerAsset>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var samplerAsset = assetServer.GetAssets<SamplerAsset>().Get(handle)
            ?? throw new InvalidOperationException("Sampler asset not found");

        var sampler = gpuContext.CreateSampler(samplerAsset.Descriptor);

        renderAssets.Add(handle, new SamplerRenderData
        {
            Sampler = sampler,
        });
    }
}