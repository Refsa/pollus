namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUColorTargetState
{
    public WGPUChainedStruct* NextInChain;
    public WGPUTextureFormat Format;
    public WGPUBlendState* Blend;
    public WGPUColorWriteMask WriteMask;
}
