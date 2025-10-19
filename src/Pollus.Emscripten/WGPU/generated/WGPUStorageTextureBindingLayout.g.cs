namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUStorageTextureBindingLayout
{
    public WGPUChainedStruct* nextInChain;
    public WGPUStorageTextureAccess access;
    public WGPUTextureFormat format;
    public WGPUTextureViewDimension viewDimension;
}
