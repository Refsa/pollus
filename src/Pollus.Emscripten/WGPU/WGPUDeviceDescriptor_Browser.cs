namespace Pollus.Emscripten;

using Silk.NET.WebGPU;

public struct WGPUDeviceDescriptor_Browser
{
    public unsafe ChainedStruct* NextInChain;
    public unsafe byte* Label;
    public nuint RequiredFeatureCount;
    public unsafe WGPUFeatureName_Browser* RequiredFeatures;
    public unsafe WGPURequiredLimits_Browser* RequiredLimits;
    public QueueDescriptor DefaultQueue;
}
