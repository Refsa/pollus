namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUImageCopyTexture
{
    public WGPUChainedStruct* NextInChain;
    public WGPUTexture* Texture;
    public uint MipLevel;
    public WGPUOrigin3D Origin;
    public WGPUTextureAspect Aspect;
}
