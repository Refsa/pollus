namespace Pollus.Engine.Rendering;

using Pollus.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

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