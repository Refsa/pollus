namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUSamplerDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUAddressMode AddressModeU;
    public WGPUAddressMode AddressModeV;
    public WGPUAddressMode AddressModeW;
    public WGPUFilterMode MagFilter;
    public WGPUFilterMode MinFilter;
    public WGPUMipmapFilterMode MipmapFilter;
    public float LodMinClamp;
    public float LodMaxClamp;
    public WGPUCompareFunction Compare;
    public ushort MaxAnisotropy;
}
