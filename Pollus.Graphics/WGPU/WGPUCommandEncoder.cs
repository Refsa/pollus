namespace Pollus.Graphics.WGPU;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

unsafe public struct WGPUCommandEncoder : IDisposable
{
    WGPUContext context;
    Silk.NET.WebGPU.CommandEncoder* native;

    public nint Native => (nint)native;

    public WGPUCommandEncoder(WGPUContext context, string label)
    {
        this.context = context;
        var labelSpan = MemoryMarshal.AsBytes(label.AsSpan());
        fixed (byte* labelPtr = labelSpan)
        {
            var descriptor = new Silk.NET.WebGPU.CommandEncoderDescriptor(label: labelPtr);
            native = context.wgpu.DeviceCreateCommandEncoder(context.device, descriptor);
        }
    }

    public void Dispose()
    {
        context.wgpu.CommandEncoderRelease(native);
    }

    public WGPUCommandBuffer Finish(string label)
    {
        var labelSpan = MemoryMarshal.AsBytes(label.AsSpan());
        fixed (byte* labelPtr = labelSpan)
        {
            var descriptor = new Silk.NET.WebGPU.CommandBufferDescriptor(label: labelPtr);
            var commandBuffer = context.wgpu.CommandEncoderFinish(native, descriptor);
            return new WGPUCommandBuffer(context, commandBuffer);
        }
    }

    public WGPURenderPassEncoder BeginRenderPass(WGPURenderPassDescriptor descriptor)
    {
        var labelSpan = MemoryMarshal.AsBytes(descriptor.Label.AsSpan());
        fixed (byte* labelPtr = labelSpan)
        {
            var wgpuDescriptor = new Silk.NET.WebGPU.RenderPassDescriptor
            {
                Label = labelPtr,
                ColorAttachmentCount = (uint)descriptor.ColorAttachments.Length,
                ColorAttachments = (Silk.NET.WebGPU.RenderPassColorAttachment*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.ColorAttachments))
            };
            return new WGPURenderPassEncoder(context, native, wgpuDescriptor);
        }
    }
}
