namespace Pollus.Graphics.WGPU;

public struct WGPUDepthStencilState
{
    public static readonly WGPUDepthStencilState Default = new()
    {
        Format = Silk.NET.WebGPU.TextureFormat.Undefined,
        DepthWriteEnabled = false,
        DepthCompare = Silk.NET.WebGPU.CompareFunction.Always,
        StencilFront = WGPUStencilFaceState.Default,
        StencilBack = WGPUStencilFaceState.Default,
        StencilReadMask = 0xFFFFFFFF,
        StencilWriteMask = 0xFFFFFFFF,
        DepthBias = 0,
        DepthBiasSlopeScale = 0,
        DepthBiasClamp = 0,
    };

    public Silk.NET.WebGPU.TextureFormat Format;

    public bool DepthWriteEnabled;
    public Silk.NET.WebGPU.CompareFunction DepthCompare;
    public int DepthBias;
    public float DepthBiasSlopeScale;
    public float DepthBiasClamp;

    public WGPUStencilFaceState StencilFront;
    public WGPUStencilFaceState StencilBack;
    public uint StencilReadMask;
    public uint StencilWriteMask;
}
