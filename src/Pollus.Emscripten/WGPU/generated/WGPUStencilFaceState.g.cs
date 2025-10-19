namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUStencilFaceState
{
    public WGPUCompareFunction compare;
    public WGPUStencilOperation failOp;
    public WGPUStencilOperation depthFailOp;
    public WGPUStencilOperation passOp;
}
