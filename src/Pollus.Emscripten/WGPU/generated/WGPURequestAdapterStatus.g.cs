namespace Pollus.Emscripten.WGPU;
public enum WGPURequestAdapterStatus : int
{
    Success = 0,
    Unavailable = 1,
    Error = 2,
    Unknown = 3,
    Force32 = 2147483647,
}
