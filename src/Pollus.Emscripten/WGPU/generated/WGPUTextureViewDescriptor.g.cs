namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUTextureViewDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUTextureFormat Format;
    public WGPUTextureViewDimension Dimension;
    public uint BaseMipLevel;
    public uint MipLevelCount;
    public uint BaseArrayLayer;
    public uint ArrayLayerCount;
    public WGPUTextureAspect Aspect;
}
