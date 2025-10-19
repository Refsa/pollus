namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUTextureViewDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUTextureFormat format;
    public WGPUTextureViewDimension dimension;
    public uint baseMipLevel;
    public uint mipLevelCount;
    public uint baseArrayLayer;
    public uint arrayLayerCount;
    public WGPUTextureAspect aspect;
}
