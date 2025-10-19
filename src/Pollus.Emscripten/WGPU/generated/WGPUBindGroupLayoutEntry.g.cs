namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBindGroupLayoutEntry
{
    public WGPUChainedStruct* NextInChain;
    public uint Binding;
    public WGPUShaderStage Visibility;
    public WGPUBufferBindingLayout Buffer;
    public WGPUSamplerBindingLayout Sampler;
    public WGPUTextureBindingLayout Texture;
    public WGPUStorageTextureBindingLayout StorageTexture;
}
