namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUStorageTextureBindingLayout
{
    public WGPUChainedStruct* NextInChain;
    public WGPUStorageTextureAccess Access;
    public WGPUTextureFormat Format;
    public WGPUTextureViewDimension ViewDimension;
}
