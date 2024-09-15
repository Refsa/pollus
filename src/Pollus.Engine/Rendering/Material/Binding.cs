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
    Buffer,
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
        var renderData = renderAssets.Get<UniformRenderData>(handle);
        var uniform = renderAssets.Get(renderData.UniformBuffer);

        return BindGroupEntry.BufferEntry<T>(binding, uniform, 0);
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
        var renderAsset = renderAssets.Get<TextureRenderData>(Image);
        var textureView = renderAssets.Get(renderAsset.View);
        return BindGroupEntry.TextureEntry(binding, textureView);
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
        var renderAsset = renderAssets.Get<SamplerRenderData>(Sampler);
        var sampler = renderAssets.Get(renderAsset.Sampler);
        return BindGroupEntry.SamplerEntry(binding, sampler);
    }
}

public class BufferBinding<TElement> : IBinding
    where TElement : unmanaged, IShaderType
{
    public BindingType Type => BindingType.Buffer;
    public required ShaderStage Visibility { get; init; }
    public required BufferBindingType BufferType { get; init; }
    public required Handle<Buffer> Buffer { get; set; }

    public BindGroupEntry Binding(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, uint binding)
    {
        renderAssets.Prepare(gpuContext, assetServer, Buffer);
        var renderAsset = renderAssets.Get<BufferRenderData>(Buffer);
        var buffer = renderAssets.Get(renderAsset.Buffer);
        return BindGroupEntry.BufferEntry<TElement>(binding, buffer, 0);
    }

    public BindGroupLayoutEntry Layout(uint binding)
    {
        return BindGroupLayoutEntry.BufferEntry<TElement>(binding, Visibility, BufferType);
    }
}