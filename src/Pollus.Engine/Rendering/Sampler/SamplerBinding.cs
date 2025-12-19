namespace Pollus.Engine.Rendering;

using System.Runtime.CompilerServices;
using Core.Serialization;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;
using Serialization;

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

public class SamplerBindingSerializer : ISerializer<SamplerBinding, WorldSerializationContext>
{
    public SamplerBinding Deserialize<TReader>(ref TReader reader, in WorldSerializationContext context) where TReader : IReader, allows ref struct
    {
        var samplerPath = reader.ReadString();
        var sampler = samplerPath switch
        {
            "nearest" => context.AssetServer.GetAssets<SamplerAsset>().Add(SamplerDescriptor.Nearest),
            "linear" => context.AssetServer.GetAssets<SamplerAsset>().Add(SamplerDescriptor.Default),
            _ => context.AssetServer.Load<SamplerAsset>(samplerPath)
        };

        return new SamplerBinding()
        {
            Sampler = sampler,
            Visibility = reader.Read<ShaderStage>()
        };
    }

    public void Serialize<TWriter>(ref TWriter reader, in SamplerBinding value, in WorldSerializationContext context) where TWriter : IWriter, allows ref struct
    {
        
    }

    [ModuleInitializer]
    public static void ModuleInitializer()
    {
        SerializerLookup<WorldSerializationContext>.RegisterSerializer(new SamplerBindingSerializer());
    }
}