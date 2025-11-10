namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUSwapChainDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUTextureUsage Usage;
    public WGPUTextureFormat Format;
    public uint Width;
    public uint Height;
    public WGPUPresentMode PresentMode;
}
