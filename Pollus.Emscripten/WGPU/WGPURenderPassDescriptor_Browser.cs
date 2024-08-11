namespace Pollus.Emscripten;

using Silk.NET.WebGPU;

unsafe public struct WGPURenderPassDescriptor_Browser
{
    nint NextInChain;
    public byte* Label; // nullable
    public uint ColorAttachmentCount;
    public WGPURenderPassColorAttachment_Browser* ColorAttachments;
    public RenderPassDepthStencilAttachment* DepthStencilAttachment; // nullable
    public QuerySet OcclusionQuerySet; // nullable
    uint TimestampWriteCount;
    nint timestampWrites;
}
