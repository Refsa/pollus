namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

unsafe public struct WGPUCommandBuffer : IDisposable
{
    IWGPUContext context;
    CommandBuffer* native;

    public nint Native => (nint)native;

    public WGPUCommandBuffer(IWGPUContext context, CommandBuffer* buffer)
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
        context.wgpu.QueueSubmit(context.Queue, 1, ref native);
    }
}