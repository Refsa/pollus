namespace Pollus.Emscripten.WGPU;
public enum WGPUQueueWorkDoneStatus : int
{
    Success = 0,
    Error = 1,
    Unknown = 2,
    DeviceLost = 3,
    Force32 = 2147483647,
}
