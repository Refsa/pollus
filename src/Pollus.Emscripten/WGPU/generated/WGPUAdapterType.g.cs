namespace Pollus.Emscripten.WGPU;
public enum WGPUAdapterType : int
{
    DiscreteGPU = 1,
    IntegratedGPU = 2,
    CPU = 3,
    Unknown = 4,
    Force32 = 2147483647,
}
