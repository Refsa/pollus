namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

unsafe public struct WGPUCommandBuffer : IDisposable
{
    WGPUContext context;
    CommandBuffer* native;

    public nint Native => (nint)native;

    public WGPUCommandBuffer(WGPUContext context, CommandBuffer* buffer)
    {
        this.context = context;
        native = buffer;
    }

    public void Dispose()
    {
        context.wgpu.CommandBufferRelease(native);
    }

    public void Submit()
    {
        context.wgpu.QueueSubmit(context.queue, 1, ref native);
    }
}