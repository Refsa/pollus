namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBindGroupLayoutEntry
{
    public WGPUChainedStruct* nextInChain;
    public uint binding;
    public WGPUShaderStage visibility;
    public WGPUBufferBindingLayout buffer;
    public WGPUSamplerBindingLayout sampler;
    public WGPUTextureBindingLayout texture;
    public WGPUStorageTextureBindingLayout storageTexture;
}
