namespace Pollus.Graphics.Rendering;

public struct DepthStencilState
{
    public static readonly DepthStencilState Default = new()
    {
        Format = TextureFormat.Undefined,
        DepthWriteEnabled = false,
        DepthCompare = Silk.NET.WebGPU.CompareFunction.Always,
        StencilFront = StencilFaceState.Default,
        StencilBack = StencilFaceState.Default,
        StencilReadMask = 0xFFFFFFFF,
        StencilWriteMask = 0xFFFFFFFF,
        DepthBias = 0,
        DepthBiasSlopeScale = 0,
        DepthBiasClamp = 0,
    };

    public TextureFormat Format;

    public bool DepthWriteEnabled;
    public Silk.NET.WebGPU.CompareFunction DepthCompare;
    public int DepthBias;
    public float DepthBiasSlopeScale;
    public float DepthBiasClamp;

    public StencilFaceState StencilFront;
    public StencilFaceState StencilBack;
    public uint StencilReadMask;
    public uint StencilWriteMask;
}
