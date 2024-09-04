namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public enum BindingType
{
    Uniform,
    Texture,
    Sampler,
}

public interface IBinding
{
    BindingType Type { get; }
    ShaderStage Visibility { get; }

    BindGroupLayoutEntry Layout(uint binding);
    BindGroupEntry Binding(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, uint binding);
}

public class UniformBinding<T> : IBinding
    where T : unmanaged, IShaderType
{
    public BindingType Type => BindingType.Uniform;

    public ShaderStage Visibility { get; init; } = ShaderStage.Vertex | ShaderStage.Fragment;

    public BindGroupLayoutEntry Layout(uint binding) => BindGroupLayoutEntry.Uniform<T>(binding, Visibility, false);
    public BindGroupEntry Binding(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, uint binding)
    {
        var handle = new Handle<UniformAsset<T>>(0);
        renderAssets.Prepare(gpuContext, assetServer, handle);
        var uniform = renderAssets.Get<UniformRenderData>(handle);

        return BindGroupEntry.BufferEntry<T>(binding, uniform.UniformBuffer, 0);
    }
}

public class TextureBinding : IBinding
{
    public BindingType Type => BindingType.Texture;

    public required Handle<ImageAsset> Image { get; set; }
    public ShaderStage Visibility { get; init; } = ShaderStage.Fragment;

    public static implicit operator TextureBinding(Handle<ImageAsset> image) => new() { Image = image };

    public BindGroupLayoutEntry Layout(uint binding) => BindGroupLayoutEntry.TextureEntry(binding, Visibility, TextureSampleType.Float, TextureViewDimension.Dimension2D);
    public BindGroupEntry Binding(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, uint binding)
    {
        renderAssets.Prepare(gpuContext, assetServer, Image);
        var texture = renderAssets.Get<TextureRenderData>(Image);
        return BindGroupEntry.TextureEntry(binding, texture.View);
    }
}

public class SamplerBinding : IBinding
{
    public BindingType Type => BindingType.Sampler;

    public required Handle<SamplerAsset> Sampler { get; set; }
    public ShaderStage Visibility { get; init; } = ShaderStage.Fragment;

    public static implicit operator SamplerBinding(Handle<SamplerAsset> sampler) => new() { Sampler = sampler };

    public BindGroupLayoutEntry Layout(uint binding) => BindGroupLayoutEntry.SamplerEntry(binding, Visibility, SamplerBindingType.Filtering);
    public BindGroupEntry Binding(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, uint binding)
    {
        renderAssets.Prepare(gpuContext, assetServer, Sampler);
        var sampler = renderAssets.Get<SamplerRenderData>(Sampler);
        return BindGroupEntry.SamplerEntry(binding, sampler.Sampler);
    }
}