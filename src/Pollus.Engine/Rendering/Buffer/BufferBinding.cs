namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class StorageBufferBinding<TElement> : IBinding
    where TElement : unmanaged, IShaderType
{
    public BindingType Type => BindingType.Buffer;
    public required ShaderStage Visibility { get; init; }
    public required BufferBindingType BufferType { get; init; }
    public required Handle<StorageBuffer> Buffer { get; set; }

    public BindGroupEntry Binding(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, uint binding)
    {
        renderAssets.Prepare(gpuContext, assetServer, Buffer);
        var renderAsset = renderAssets.Get<StorageBufferRenderData>(Buffer);
        var buffer = renderAssets.Get(renderAsset.Buffer);
        return BindGroupEntry.BufferEntry<TElement>(binding, buffer, 0);
    }

    public BindGroupLayoutEntry Layout(uint binding)
    {
        return BindGroupLayoutEntry.BufferEntry<TElement>(binding, Visibility, BufferType);
    }
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