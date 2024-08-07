namespace Pollus.Graphics.WGPU;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Utils;

unsafe public struct WGPUCommandEncoder : IDisposable
{
    IWGPUContext context;
    Silk.NET.WebGPU.CommandEncoder* native;

    public nint Native => (nint)native;

    public WGPUCommandEncoder(IWGPUContext context, string label)
    {
        this.context = context;
        var labelPin = TemporaryPin.PinString(label);
        var descriptor = new Silk.NET.WebGPU.CommandEncoderDescriptor(label: (byte*)labelPin.Ptr);
        native = context.wgpu.DeviceCreateCommandEncoder(context.Device, descriptor);
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

#if BROWSER
        var colorAttachments = new Browser.WGPURenderPassColorAttachment_Browser[descriptor.ColorAttachments.Length];
        for (int i = 0; i < descriptor.ColorAttachments.Length; i++)
        {
            colorAttachments[i] = new Browser.WGPURenderPassColorAttachment_Browser
            {
                View = (Silk.NET.WebGPU.TextureView*)descriptor.ColorAttachments[i].View,
                ResolveTarget = (Silk.NET.WebGPU.TextureView*)descriptor.ColorAttachments[i].ResolveTarget,
                LoadOp = descriptor.ColorAttachments[i].LoadOp,
                StoreOp = descriptor.ColorAttachments[i].StoreOp,
                ClearValue = descriptor.ColorAttachments[i].ClearValue,
            };
        }
        using var colorAttachmentsPtr = TemporaryPin.Pin(colorAttachments);
        var wgpuDescriptor = new Browser.WGPURenderPassDescriptor_Browser
        {
            Label = (byte*)label.Ptr,
            ColorAttachmentCount = (uint)descriptor.ColorAttachments.Length,
            ColorAttachments = (Browser.WGPURenderPassColorAttachment_Browser*)colorAttachmentsPtr.Ptr
        };
#else
        var wgpuDescriptor = new Silk.NET.WebGPU.RenderPassDescriptor
        {
            Label = (byte*)label.Ptr,
            ColorAttachmentCount = (uint)descriptor.ColorAttachments.Length,
            ColorAttachments = (Silk.NET.WebGPU.RenderPassColorAttachment*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.ColorAttachments))
        };
#endif

        return new WGPURenderPassEncoder(context, native, wgpuDescriptor);
    }
}
