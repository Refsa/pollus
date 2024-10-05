namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class UniformBinding<T> : IBinding
    where T : unmanaged, IShaderType
{
    public BindingType Type => BindingType.Uniform;

    public ShaderStage Visibility { get; init; } = ShaderStage.Vertex | ShaderStage.Fragment;

    public BindGroupLayoutEntry Layout(uint binding) => BindGroupLayoutEntry.Uniform<T>(binding, Visibility, false);
    public BindGroupEntry Binding(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, uint binding)
    {
        var handle = new Handle<Uniform<T>>(0);
        renderAssets.Prepare(gpuContext, assetServer, handle);
        var renderData = renderAssets.Get<UniformRenderData>(handle);
        var uniform = renderAssets.Get(renderData.UniformBuffer);

        return BindGroupEntry.BufferEntry<T>(binding, uniform, 0);
    }
}