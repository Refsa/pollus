namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class TextureBinding : IBinding
{
    public BindingType Type => BindingType.Texture;

    public required Handle<Texture2D> Image { get; set; }
    public ShaderStage Visibility { get; init; } = ShaderStage.Fragment;

    public static implicit operator TextureBinding(Handle<Texture2D> image) => new() { Image = image };

    public BindGroupLayoutEntry Layout(uint binding) => BindGroupLayoutEntry.TextureEntry(binding, Visibility, TextureSampleType.Float, TextureViewDimension.Dimension2D);
    public BindGroupEntry Binding(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, uint binding)
    {
        renderAssets.Prepare(gpuContext, assetServer, Image);
        var renderAsset = renderAssets.Get<TextureRenderData>(Image);
        var textureView = renderAssets.Get(renderAsset.View);
        return BindGroupEntry.TextureEntry(binding, textureView);
    }
}