namespace Pollus.Emscripten.WGPU;
public enum WGPUTextureUsage : int
{
    None = 0,
    CopySrc = 1,
    CopyDst = 2,
    TextureBinding = 4,
    StorageBinding = 8,
    RenderAttachment = 16,
    Force32 = 2147483647,
}
