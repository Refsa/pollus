namespace Pollus.Emscripten.WGPU;
public enum WGPUCompilationInfoRequestStatus : int
{
    Success = 0,
    Error = 1,
    DeviceLost = 2,
    Unknown = 3,
    Force32 = 2147483647,
}
