namespace Pollus.Emscripten.WGPU;
public enum WGPUBufferMapState : int
{
    Unmapped = 1,
    Pending = 2,
    Mapped = 3,
    Force32 = 2147483647,
}
