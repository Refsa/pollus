namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;

unsafe public struct GPUCommandBuffer : IDisposable
{
    IWGPUContext context;
    Silk.NET.WebGPU.CommandBuffer* native;

    public nint Native => (nint)native;

    public GPUCommandBuffer(IWGPUContext context, Silk.NET.WebGPU.CommandBuffer* buffer)
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