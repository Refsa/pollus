namespace Pollus.Engine.Rendering;

using Core.Serialization;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

[Serialize]
public partial class SamplerBinding : IBinding
{
    public static SamplerBinding Default => new() { Sampler = Handle<SamplerAsset>.Null };

    public required Handle<SamplerAsset> Sampler { get; set; }
    public ShaderStage Visibility { get; set; } = ShaderStage.Fragment;
    public BindingType Type => BindingType.Sampler;

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