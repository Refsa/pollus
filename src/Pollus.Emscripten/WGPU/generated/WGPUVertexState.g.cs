namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUVertexState
{
    public WGPUChainedStruct* NextInChain;
    public WGPUShaderModule Module;
    public byte* EntryPoint;
    public nuint ConstantCount;
    public WGPUConstantEntry* Constants;
    public nuint BufferCount;
    public WGPUVertexBufferLayout* Buffers;
}
