namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUVertexBufferLayout
{
    public ulong ArrayStride;
    public WGPUVertexStepMode StepMode;
    public nuint AttributeCount;
    public WGPUVertexAttribute* Attributes;
}
