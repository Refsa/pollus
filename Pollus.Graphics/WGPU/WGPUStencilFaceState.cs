namespace Pollus.Graphics.WGPU;

public struct WGPUStencilFaceState
{
    public static readonly WGPUStencilFaceState Default = new()
    {
        Compare = Silk.NET.WebGPU.CompareFunction.Always,
        FailOp = Silk.NET.WebGPU.StencilOperation.Keep,
        DepthFailOp = Silk.NET.WebGPU.StencilOperation.Keep,
        PassOp = Silk.NET.WebGPU.StencilOperation.Keep,
    };

    public Silk.NET.WebGPU.CompareFunction Compare;
    public Silk.NET.WebGPU.StencilOperation FailOp;
    public Silk.NET.WebGPU.StencilOperation DepthFailOp;
    public Silk.NET.WebGPU.StencilOperation PassOp;
}
