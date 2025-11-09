namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUDeviceDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public nuint RequiredFeatureCount;
    public WGPUFeatureName* RequiredFeatures;
    public WGPURequiredLimits* RequiredLimits;
    public WGPUQueueDescriptor DefaultQueue;
    public WGPUDeviceLostCallback DeviceLostCallback;
    public void* DeviceLostUserdata;
}
