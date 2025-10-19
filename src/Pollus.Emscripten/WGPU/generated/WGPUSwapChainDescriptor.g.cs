namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUSwapChainDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUTextureUsage usage;
    public WGPUTextureFormat format;
    public uint width;
    public uint height;
    public WGPUPresentMode presentMode;
}
