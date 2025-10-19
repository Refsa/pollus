namespace Pollus.Emscripten.WGPU;
public enum WGPUStencilOperation : int
{
    Undefined = 0,
    Keep = 1,
    Zero = 2,
    Replace = 3,
    Invert = 4,
    IncrementClamp = 5,
    DecrementClamp = 6,
    IncrementWrap = 7,
    DecrementWrap = 8,
    Force32 = 2147483647,
}
