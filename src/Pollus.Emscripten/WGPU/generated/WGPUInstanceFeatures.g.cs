namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUInstanceFeatures
{
    public WGPUChainedStruct* nextInChain;
    public bool timedWaitAnyEnable;
    public nuint timedWaitAnyMaxCount;
}
