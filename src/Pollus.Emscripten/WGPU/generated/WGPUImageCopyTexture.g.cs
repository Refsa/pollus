namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUImageCopyTexture
{
    public WGPUChainedStruct* nextInChain;
    public WGPUTexture texture;
    public uint mipLevel;
    public WGPUOrigin3D origin;
    public WGPUTextureAspect aspect;
}
