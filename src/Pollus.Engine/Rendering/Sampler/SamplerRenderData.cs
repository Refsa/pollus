namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class SamplerRenderData
{
    public required Handle<GPUSampler> Sampler { get; init; }
}

public class SamplerRenderDataLoader : IRenderDataLoader
{
    public int TargetType => TypeLookup.ID<SamplerAsset>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var samplerAsset = assetServer.GetAssets<SamplerAsset>().Get(handle)
            ?? throw new InvalidOperationException("Sampler asset not found");

        var sampler = gpuContext.CreateSampler(samplerAsset.Descriptor);

        renderAssets.Add(handle, new SamplerRenderData
        {
            Sampler = renderAssets.Add(sampler),
        });
    }
}