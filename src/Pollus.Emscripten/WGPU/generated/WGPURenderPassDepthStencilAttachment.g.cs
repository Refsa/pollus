namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPassDepthStencilAttachment
{
    public WGPUTextureView view;
    public WGPULoadOp depthLoadOp;
    public WGPUStoreOp depthStoreOp;
    public float depthClearValue;
    public bool depthReadOnly;
    public WGPULoadOp stencilLoadOp;
    public WGPUStoreOp stencilStoreOp;
    public uint stencilClearValue;
    public bool stencilReadOnly;
}
