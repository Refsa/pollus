namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUTextureDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUTextureUsage usage;
    public WGPUTextureDimension dimension;
    public WGPUExtent3D size;
    public WGPUTextureFormat format;
    public uint mipLevelCount;
    public uint sampleCount;
    public nuint viewFormatCount;
    public WGPUTextureFormat* viewFormats;
}
