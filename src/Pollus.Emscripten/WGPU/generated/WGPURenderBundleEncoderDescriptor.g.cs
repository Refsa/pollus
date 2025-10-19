namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderBundleEncoderDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public nuint ColorFormatCount;
    public WGPUTextureFormat* ColorFormats;
    public WGPUTextureFormat DepthStencilFormat;
    public uint SampleCount;
    public bool DepthReadOnly;
    public bool StencilReadOnly;
}
