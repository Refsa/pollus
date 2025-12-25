namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class TextureRenderData
{
    public required Handle<GPUTexture> Texture { get; init; }
    public required Handle<GPUTextureView> View { get; init; }
}

public class TextureRenderDataLoader<TTexture> : IRenderDataLoader
    where TTexture : ITexture
{
    public TypeID TargetType => TypeLookup.ID<TTexture>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var textureAsset = assetServer.GetAssets<TTexture>().Get(handle)
            ?? throw new InvalidOperationException("Image asset not found");

        var texture = gpuContext.CreateTexture(new()
        {
            Label = textureAsset.Name,
            Usage = TextureUsage.CopyDst | TextureUsage.TextureBinding,
            Dimension = textureAsset.Dimension,
            Size = new Extent3D { Width = textureAsset.Width, Height = textureAsset.Height, DepthOrArrayLayers = textureAsset.Depth },
            Format = textureAsset.Format,
            MipLevelCount = textureAsset.MipCount,
            SampleCount = textureAsset.SampleCount,
        });
        texture.Write(textureAsset.Data);

        var textureView = texture.GetTextureView();
        renderAssets.Add(handle, new TextureRenderData
        {
            Texture = renderAssets.Add(texture),
            View = renderAssets.Add(textureView),
        });
    }

    public void Unload(RenderAssets renderAssets, Handle handle)
    {
        var texture = renderAssets.Get<TextureRenderData>(handle);
        renderAssets.Unload(texture.Texture);
        renderAssets.Unload(texture.View);
    }
}