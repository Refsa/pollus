namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUDeviceDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public nuint RequiredFeatureCount;
    public WGPUFeatureName* RequiredFeatures;
    public WGPURequiredLimits* RequiredLimits;
    public WGPUQueueDescriptor DefaultQueue;
    public delegate* unmanaged[Cdecl]<WGPUDeviceLostReason, byte*, void*, void> DeviceLostCallback;
    public void* DeviceLostUserdata;
}
