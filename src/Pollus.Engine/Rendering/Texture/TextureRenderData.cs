namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

public class TextureRenderData : IRenderData
{
    public required GPUTexture Texture { get; init; }
    public required GPUTextureView View { get; init; }

    public void Dispose()
    {
        Texture.Dispose();
    }
}

public class TextureRenderDataLoader : IRenderDataLoader
{
    public int TargetType => AssetLookup.ID<ImageAsset>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var imageAsset = assetServer.GetAssets<ImageAsset>().Get(handle)
            ?? throw new InvalidOperationException("Image asset not found");

        var texture = gpuContext.CreateTexture(TextureDescriptor.D2(
            imageAsset.Name,
            TextureUsage.TextureBinding | TextureUsage.CopyDst,
            TextureFormat.Rgba8Unorm,
            new Vec2<uint>(imageAsset.Width, imageAsset.Height)
        ));
        texture.Write(imageAsset.Data);

        renderAssets.Add(handle, new TextureRenderData
        {
            Texture = texture,
            View = texture.GetTextureView(),
        });
    }
}