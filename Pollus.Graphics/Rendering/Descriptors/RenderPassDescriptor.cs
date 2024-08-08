namespace Pollus.Graphics.Rendering;

public ref struct RenderPassDescriptor
{
    public string Label;
    public ReadOnlySpan<RenderPassColorAttachment> ColorAttachments;
    public RenderPassDepthStencilAttachment? DepthStencilAttachment;
}
