namespace Pollus.Emscripten.WGPU;
public enum WGPUTextureAspect : int
{
    Undefined = 0,
    All = 1,
    StencilOnly = 2,
    DepthOnly = 3,
    Force32 = 2147483647,
}
