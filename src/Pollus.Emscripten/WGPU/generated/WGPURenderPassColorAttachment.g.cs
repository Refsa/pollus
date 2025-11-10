namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPassColorAttachment
{
    public WGPUChainedStruct* NextInChain;
    public WGPUTextureView* View;
    public uint DepthSlice;
    public WGPUTextureView* ResolveTarget;
    public WGPULoadOp LoadOp;
    public WGPUStoreOp StoreOp;
    public WGPUColor ClearValue;
}
