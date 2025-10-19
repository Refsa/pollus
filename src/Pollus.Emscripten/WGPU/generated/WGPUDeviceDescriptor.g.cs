namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUDeviceDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public nuint requiredFeatureCount;
    public WGPUFeatureName* requiredFeatures;
    public WGPURequiredLimits* requiredLimits;
    public WGPUQueueDescriptor defaultQueue;
    public WGPUDeviceLostCallback deviceLostCallback;
    public void* deviceLostUserdata;
}
