namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUImageCopyBuffer
{
    public WGPUChainedStruct* nextInChain;
    public WGPUTextureDataLayout layout;
    public WGPUBuffer buffer;
}
