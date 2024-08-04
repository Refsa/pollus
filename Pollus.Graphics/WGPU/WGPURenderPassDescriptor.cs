namespace Pollus.Graphics.WGPU;
using Silk.NET.WebGPU;

public ref struct WGPURenderPassDescriptor
{
    public string Label;
    public ReadOnlySpan<WGPURenderPassColorAttachment> ColorAttachments;
    public WGPURenderPassDepthStencilAttachment? DepthStencilAttachment;
}
