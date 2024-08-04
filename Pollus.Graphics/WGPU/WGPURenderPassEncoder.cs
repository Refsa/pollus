namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

unsafe public struct WGPURenderPassEncoder : IDisposable
{
    WGPUContext context;
    public RenderPassEncoder* Native;

    public WGPURenderPassEncoder(WGPUContext context, CommandEncoder* commandEncoder, RenderPassDescriptor descriptor)
    {
        this.context = context;
        Native = context.wgpu.CommandEncoderBeginRenderPass(commandEncoder, descriptor);
    }

    public void Dispose()
    {
        context.wgpu.RenderPassEncoderRelease(Native);
    }


    public void End()
    {
        context.wgpu.RenderPassEncoderEnd(Native);
    }
}
