namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUDepthStencilState
{
    public WGPUChainedStruct* nextInChain;
    public WGPUTextureFormat format;
    public bool depthWriteEnabled;
    public WGPUCompareFunction depthCompare;
    public WGPUStencilFaceState stencilFront;
    public WGPUStencilFaceState stencilBack;
    public uint stencilReadMask;
    public uint stencilWriteMask;
    public int depthBias;
    public float depthBiasSlopeScale;
    public float depthBiasClamp;
}
