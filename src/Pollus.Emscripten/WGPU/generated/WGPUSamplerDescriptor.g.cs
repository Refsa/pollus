namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUSamplerDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUAddressMode addressModeU;
    public WGPUAddressMode addressModeV;
    public WGPUAddressMode addressModeW;
    public WGPUFilterMode magFilter;
    public WGPUFilterMode minFilter;
    public WGPUMipmapFilterMode mipmapFilter;
    public float lodMinClamp;
    public float lodMaxClamp;
    public WGPUCompareFunction compare;
    public ushort maxAnisotropy;
}
