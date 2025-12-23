namespace Pollus.Graphics.Rendering;

using Core.Serialization;

[Serialize]
public partial record struct SamplerDescriptor
{
    public static readonly SamplerDescriptor Default = new()
    {
        Label = "sampler",
        AddressModeU = Silk.NET.WebGPU.AddressMode.ClampToEdge,
        AddressModeV = Silk.NET.WebGPU.AddressMode.ClampToEdge,
        AddressModeW = Silk.NET.WebGPU.AddressMode.ClampToEdge,
        MagFilter = Silk.NET.WebGPU.FilterMode.Linear,
        MinFilter = Silk.NET.WebGPU.FilterMode.Linear,
        MipmapFilter = Silk.NET.WebGPU.MipmapFilterMode.Linear,
        LodMinClamp = 0,
        LodMaxClamp = 1000,
        Compare = Silk.NET.WebGPU.CompareFunction.Undefined,
        MaxAnisotropy = 1,
    };

    public static readonly SamplerDescriptor Nearest = Default with
    {
        MagFilter = Silk.NET.WebGPU.FilterMode.Nearest,
        MinFilter = Silk.NET.WebGPU.FilterMode.Nearest,
    };

    public string Label;

    public Silk.NET.WebGPU.AddressMode AddressModeU;
    public Silk.NET.WebGPU.AddressMode AddressModeV;
    public Silk.NET.WebGPU.AddressMode AddressModeW;

    public Silk.NET.WebGPU.FilterMode MagFilter;
    public Silk.NET.WebGPU.FilterMode MinFilter;

    public Silk.NET.WebGPU.MipmapFilterMode MipmapFilter;

    public float LodMinClamp;
    public float LodMaxClamp;

    public Silk.NET.WebGPU.CompareFunction Compare;

    public ushort MaxAnisotropy;
}
