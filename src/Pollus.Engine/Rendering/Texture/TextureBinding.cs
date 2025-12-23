namespace Pollus.Engine.Rendering;

using Core.Serialization;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

[Serialize]
public partial class TextureBinding : IBinding
{
    public static TextureBinding Default => new() { Image = Handle<Texture2D>.Null };

    public required Handle<Texture2D> Image { get; set; }
    public ShaderStage Visibility { get; set; } = ShaderStage.Fragment;
    public BindingType Type => BindingType.Texture;

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