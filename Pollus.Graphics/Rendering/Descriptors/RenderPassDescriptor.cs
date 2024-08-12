namespace Pollus.Graphics.Rendering;

public ref struct RenderPassDescriptor
{
    public ReadOnlySpan<char> Label;
    public ReadOnlySpan<RenderPassColorAttachment> ColorAttachments;
    public RenderPassDepthStencilAttachment? DepthStencilAttachment;
}
