namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUTextureBindingLayout
{
    public WGPUChainedStruct* nextInChain;
    public WGPUTextureSampleType sampleType;
    public WGPUTextureViewDimension viewDimension;
    public bool multisampled;
}
