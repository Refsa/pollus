namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUInstanceFeatures
{
    public WGPUChainedStruct* NextInChain;
    public bool TimedWaitAnyEnable;
    public nuint TimedWaitAnyMaxCount;
}
