namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPassColorAttachment
{
    public WGPUChainedStruct* nextInChain;
    public WGPUTextureView view;
    public uint depthSlice;
    public WGPUTextureView resolveTarget;
    public WGPULoadOp loadOp;
    public WGPUStoreOp storeOp;
    public WGPUColor clearValue;
}
