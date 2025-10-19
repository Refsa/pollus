namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderBundleEncoderDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public nuint colorFormatCount;
    public WGPUTextureFormat* colorFormats;
    public WGPUTextureFormat depthStencilFormat;
    public uint sampleCount;
    public bool depthReadOnly;
    public bool stencilReadOnly;
}
