namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

public class TextureRenderData
{
    public required Handle<GPUTexture> Texture { get; init; }
    public required Handle<GPUTextureView> View { get; init; }
}

public class TextureRenderDataLoader : IRenderDataLoader
{
    public int TargetType => TypeLookup.ID<ImageAsset>();

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

        var textureView = texture.GetTextureView();
        renderAssets.Add(handle, new TextureRenderData
        {
            Texture = renderAssets.Add(texture),
            View = renderAssets.Add(textureView),
        });
    }
}