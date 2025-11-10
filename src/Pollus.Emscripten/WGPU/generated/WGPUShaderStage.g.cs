namespace Pollus.Emscripten.WGPU;
public enum WGPUShaderStage : int
{
    None = 0,
    Vertex = 1,
    Fragment = 2,
    Compute = 4,
    Force32 = 2147483647,
}
