namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUTextureDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUTextureUsage Usage;
    public WGPUTextureDimension Dimension;
    public WGPUExtent3D Size;
    public WGPUTextureFormat Format;
    public uint MipLevelCount;
    public uint SampleCount;
    public nuint ViewFormatCount;
    public WGPUTextureFormat* ViewFormats;
}
