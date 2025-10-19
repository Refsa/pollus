namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUColorTargetState
{
    public WGPUChainedStruct* nextInChain;
    public WGPUTextureFormat format;
    public WGPUBlendState* blend;
    public WGPUColorWriteMask writeMask;
}
