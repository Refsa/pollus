namespace Pollus.Emscripten.WGPU;
public enum WGPUBufferBindingType : int
{
    Undefined = 0,
    Uniform = 1,
    Storage = 2,
    ReadOnlyStorage = 3,
    Force32 = 2147483647,
}
