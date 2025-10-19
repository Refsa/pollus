namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBufferBindingLayout
{
    public WGPUChainedStruct* nextInChain;
    public WGPUBufferBindingType type;
    public bool hasDynamicOffset;
    public ulong minBindingSize;
}
