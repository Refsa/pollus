namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPassDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public nuint colorAttachmentCount;
    public WGPURenderPassColorAttachment* colorAttachments;
    public WGPURenderPassDepthStencilAttachment* depthStencilAttachment;
    public WGPUQuerySet occlusionQuerySet;
    public WGPURenderPassTimestampWrites* timestampWrites;
}
