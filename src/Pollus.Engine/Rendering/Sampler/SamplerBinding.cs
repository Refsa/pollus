namespace Pollus.Engine.Rendering;

using System.Runtime.CompilerServices;
using Core.Assets;
using Core.Serialization;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;
using Serialization;

[Asset]
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
        var samplerPath = reader.ReadString("Sampler");
        var sampler = samplerPath switch
        {
            null => context.AssetServer.GetAssets<SamplerAsset>().Add(
                reader.Deserialize<SamplerAsset>()
            ),
            "nearest" => context.AssetServer.GetAssets<SamplerAsset>().Add(SamplerDescriptor.Nearest),
            "linear" => context.AssetServer.GetAssets<SamplerAsset>().Add(SamplerDescriptor.Default),
            _ => context.AssetServer.LoadAsync<SamplerAsset>(samplerPath)
        };

        return new SamplerBinding()
        {
            Sampler = sampler,
            Visibility = reader.Read<ShaderStage>("Visibility")
        };
    }

    public void Serialize<TWriter>(ref TWriter writer, in SamplerBinding value, in WorldSerializationContext context) where TWriter : IWriter, allows ref struct
    {
        if (context.AssetServer.GetAssets<SamplerAsset>().GetInfo(value.Sampler) is { } samplerInfo)
        {
            if (samplerInfo.Path is not null)
            {
                writer.Write(value.Sampler, "Sampler");
            }
            else if (samplerInfo.Asset is not null)
            {
                if (samplerInfo.Asset.Descriptor == SamplerDescriptor.Default)
                {
                    writer.Write("linear", "Sampler");
                }
                else if (samplerInfo.Asset.Descriptor == SamplerDescriptor.Nearest)
                {
                    writer.Write("nearest", "Sampler");
                }
                else
                {
                    writer.Serialize(samplerInfo.Asset, "Sampler");
                }
            }
        }

        writer.Write(value.Visibility, "Visibility");
    }

    [ModuleInitializer]
    public static void ModuleInitializer()
    {
        SerializerLookup<WorldSerializationContext>.RegisterSerializer(new SamplerBindingSerializer());
    }
}