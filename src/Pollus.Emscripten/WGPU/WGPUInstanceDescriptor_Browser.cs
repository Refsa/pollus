namespace Pollus.Emscripten;

public struct WGPUInstanceDescriptor_Browser
{
    public unsafe Silk.NET.WebGPU.ChainedStruct* NextInChain;
    public WGPUInstanceFeatures_Browser Features;
}

public struct WGPUInstanceFeatures_Browser
{
    public unsafe Silk.NET.WebGPU.ChainedStruct* NextInChain;
    public bool TimedWaitAnyEnable;
    public ulong TimedWaitAnyMaxCount;
}