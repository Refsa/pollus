namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUTextureBindingLayout
{
    public WGPUChainedStruct* NextInChain;
    public WGPUTextureSampleType SampleType;
    public WGPUTextureViewDimension ViewDimension;
    public bool Multisampled;
}
