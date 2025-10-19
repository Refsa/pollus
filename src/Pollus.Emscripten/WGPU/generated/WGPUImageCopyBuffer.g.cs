namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUImageCopyBuffer
{
    public WGPUChainedStruct* NextInChain;
    public WGPUTextureDataLayout Layout;
    public WGPUBuffer Buffer;
}
