namespace Pollus.Emscripten.WGPU;
public enum WGPUVertexStepMode : int
{
    Undefined = 0,
    VertexBufferNotUsed = 1,
    Vertex = 2,
    Instance = 3,
    Force32 = 2147483647,
}
