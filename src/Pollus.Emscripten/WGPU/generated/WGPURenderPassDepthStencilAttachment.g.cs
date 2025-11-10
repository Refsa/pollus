namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPassDepthStencilAttachment
{
    public WGPUTextureView* View;
    public WGPULoadOp DepthLoadOp;
    public WGPUStoreOp DepthStoreOp;
    public float DepthClearValue;
    public bool DepthReadOnly;
    public WGPULoadOp StencilLoadOp;
    public WGPUStoreOp StencilStoreOp;
    public uint StencilClearValue;
    public bool StencilReadOnly;
}
