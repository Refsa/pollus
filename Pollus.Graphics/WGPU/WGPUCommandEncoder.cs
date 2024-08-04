namespace Pollus.Graphics.WGPU;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Utils;

unsafe public struct WGPUCommandEncoder : IDisposable
{
    WGPUContext context;
    Silk.NET.WebGPU.CommandEncoder* native;

    public nint Native => (nint)native;

    public WGPUCommandEncoder(WGPUContext context, string label)
    {
        this.context = context;
        var labelPin = TemporaryPin.PinString(label);
        var descriptor = new Silk.NET.WebGPU.CommandEncoderDescriptor(label: (byte*)labelPin.Ptr);
        native = context.wgpu.DeviceCreateCommandEncoder(context.device, descriptor);
    }

    public void Dispose()
    {
        context.wgpu.CommandEncoderRelease(native);
    }

    public WGPUCommandBuffer Finish(string label)
    {
        using var labelPin = TemporaryPin.PinString(label);
        var descriptor = new Silk.NET.WebGPU.CommandBufferDescriptor(label: (byte*)labelPin.Ptr);
        var commandBuffer = context.wgpu.CommandEncoderFinish(native, descriptor);
        return new WGPUCommandBuffer(context, commandBuffer);
    }

    public WGPURenderPassEncoder BeginRenderPass(WGPURenderPassDescriptor descriptor)
    {
        using var label = TemporaryPin.PinString(descriptor.Label);
        var wgpuDescriptor = new Silk.NET.WebGPU.RenderPassDescriptor
        {
            Label = (byte*)label.Ptr,
            ColorAttachmentCount = (uint)descriptor.ColorAttachments.Length,
            ColorAttachments = (Silk.NET.WebGPU.RenderPassColorAttachment*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.ColorAttachments))
        };
        return new WGPURenderPassEncoder(context, native, wgpuDescriptor);
    }
}
