namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBufferBindingLayout
{
    public WGPUChainedStruct* NextInChain;
    public WGPUBufferBindingType Type;
    public bool HasDynamicOffset;
    public ulong MinBindingSize;
}
