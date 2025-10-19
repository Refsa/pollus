namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUStencilFaceState
{
    public WGPUCompareFunction Compare;
    public WGPUStencilOperation FailOp;
    public WGPUStencilOperation DepthFailOp;
    public WGPUStencilOperation PassOp;
}
