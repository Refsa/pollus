namespace Pollus.Graphics.Rendering;

public struct SamplerDescriptor
{
    public readonly string Label;

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
