namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUVertexBufferLayout
{
    public ulong arrayStride;
    public WGPUVertexStepMode stepMode;
    public nuint attributeCount;
    public WGPUVertexAttribute* attributes;
}
