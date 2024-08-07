namespace Pollus.Graphics.WGPU.Browser;
using Silk.NET.WebGPU;

public struct WGPUDeviceDescriptor_Browser
{
    public unsafe ChainedStruct* NextInChain;
    public unsafe byte* Label;
    public nuint RequiredFeatureCount;
    public unsafe FeatureName* RequiredFeatures;
    public unsafe WGPURequiredLimits_Browser* RequiredLimits;
    public QueueDescriptor DefaultQueue;
}
