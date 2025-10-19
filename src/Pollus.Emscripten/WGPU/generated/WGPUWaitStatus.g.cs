namespace Pollus.Emscripten.WGPU;
public enum WGPUWaitStatus : int
{
    Success = 0,
    TimedOut = 1,
    UnsupportedTimeout = 2,
    UnsupportedCount = 3,
    UnsupportedMixedSources = 4,
    Unknown = 5,
    Force32 = 2147483647,
}
