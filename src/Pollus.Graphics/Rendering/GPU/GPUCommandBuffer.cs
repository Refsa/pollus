namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public struct GPUCommandBuffer : IDisposable
{
    IWGPUContext context;
    NativeHandle<CommandBufferTag> native;

    public NativeHandle<CommandBufferTag> Native => native;

    public GPUCommandBuffer(IWGPUContext context, NativeHandle<CommandBufferTag> buffer)
    {
        this.context = context;
        native = buffer;
    }

    public void Dispose()
    {
        context.Backend.CommandBufferRelease(native);
    }

    public void Submit()
    {
        Span<NativeHandle<CommandBufferTag>> one = stackalloc NativeHandle<CommandBufferTag>[1];
        one[0] = native;
        context.Backend.QueueSubmit(context.QueueHandle, one);
    }
}