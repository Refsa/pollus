namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

unsafe public struct WGPUCommandBuffer : IDisposable
{
    public WGPUContext context;
    public CommandBuffer* Buffer;

    public WGPUCommandBuffer(WGPUContext context, CommandBuffer* buffer)
    {
        this.context = context;
        Buffer = buffer;
    }

    public void Dispose()
    {
        context.wgpu.CommandBufferRelease(Buffer);
    }

    public void Submit()
    {
        context.wgpu.QueueSubmit(context.queue, 1, ref Buffer);
    }
}