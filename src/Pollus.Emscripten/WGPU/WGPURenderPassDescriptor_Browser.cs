#pragma warning disable CS0169

namespace Pollus.Emscripten;

using Silk.NET.WebGPU;

unsafe public struct WGPURenderPassDescriptor_Browser
{
    nint nextInChain;
    public byte* Label; // nullable
    public uint ColorAttachmentCount;
    public WGPURenderPassColorAttachment_Browser* ColorAttachments;
    public RenderPassDepthStencilAttachment* DepthStencilAttachment; // nullable
    public QuerySet OcclusionQuerySet; // nullable
    uint timestampWriteCount;
    nint timestampWrites;
}

#pragma warning restore CS0169