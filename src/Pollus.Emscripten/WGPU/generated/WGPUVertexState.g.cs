namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUVertexState
{
    public WGPUChainedStruct* nextInChain;
    public WGPUShaderModule module;
    public char* entryPoint;
    public nuint constantCount;
    public WGPUConstantEntry* constants;
    public nuint bufferCount;
    public WGPUVertexBufferLayout* buffers;
}
