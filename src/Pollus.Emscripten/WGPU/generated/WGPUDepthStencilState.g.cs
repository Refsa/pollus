namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUDepthStencilState
{
    public WGPUChainedStruct* NextInChain;
    public WGPUTextureFormat Format;
    public bool DepthWriteEnabled;
    public WGPUCompareFunction DepthCompare;
    public WGPUStencilFaceState StencilFront;
    public WGPUStencilFaceState StencilBack;
    public uint StencilReadMask;
    public uint StencilWriteMask;
    public int DepthBias;
    public float DepthBiasSlopeScale;
    public float DepthBiasClamp;
}
