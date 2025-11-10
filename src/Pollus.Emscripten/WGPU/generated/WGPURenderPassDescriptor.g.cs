namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPassDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public nuint ColorAttachmentCount;
    public WGPURenderPassColorAttachment* ColorAttachments;
    public WGPURenderPassDepthStencilAttachment* DepthStencilAttachment;
    public WGPUQuerySet* OcclusionQuerySet;
    public WGPURenderPassTimestampWrites* TimestampWrites;
}
