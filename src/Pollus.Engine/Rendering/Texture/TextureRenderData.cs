namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

public class TextureRenderData : IRenderData
{
    public required GPUTexture Texture { get; init; }
    public required GPUSampler Sampler { get; init; }

    public void Dispose()
    {
        Texture.Dispose();
        Sampler.Dispose();
    }

    public static TextureRenderData Create(IWGPUContext gpuContext, ImageAsset imageAsset)
    {
        var texture = gpuContext.CreateTexture(TextureDescriptor.D2(
            imageAsset.Name,
            TextureUsage.TextureBinding | TextureUsage.CopyDst,
            TextureFormat.Rgba8Unorm,
            new Vec2<uint>(imageAsset.Width, imageAsset.Height)
        ));
        texture.Write(imageAsset.Data);

        var sampler = gpuContext.CreateSampler(SamplerDescriptor.Nearest);

        return new TextureRenderData
        {
            Texture = texture,
            Sampler = sampler,
        };
    }
}