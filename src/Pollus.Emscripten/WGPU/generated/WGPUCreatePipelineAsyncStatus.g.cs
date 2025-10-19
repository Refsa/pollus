namespace Pollus.Emscripten.WGPU;
public enum WGPUCreatePipelineAsyncStatus : int
{
    Success = 0,
    ValidationError = 1,
    InternalError = 2,
    DeviceLost = 3,
    DeviceDestroyed = 4,
    Unknown = 5,
    Force32 = 2147483647,
}
