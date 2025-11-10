namespace Pollus.Emscripten.WGPU;
public enum WGPUErrorType : int
{
    NoError = 0,
    Validation = 1,
    OutOfMemory = 2,
    Internal = 3,
    Unknown = 4,
    DeviceLost = 5,
    Force32 = 2147483647,
}
