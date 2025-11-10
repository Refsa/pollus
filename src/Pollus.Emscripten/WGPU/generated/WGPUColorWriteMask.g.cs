namespace Pollus.Emscripten.WGPU;
public enum WGPUColorWriteMask : int
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Alpha = 8,
    All = 15,
    Force32 = 2147483647,
}
