namespace Pollus.Graphics.Rendering;

public ref struct RenderPassDescriptor
{
    public ReadOnlySpan<char> Label;
    public GPUTimestampWrites TimestampWrites;
    public ReadOnlySpan<RenderPassColorAttachment> ColorAttachments;
    public RenderPassDepthStencilAttachment? DepthStencilAttachment;
}
