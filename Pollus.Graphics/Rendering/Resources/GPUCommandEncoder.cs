namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

unsafe public struct GPUCommandEncoder : IDisposable
{
    IWGPUContext context;
    Silk.NET.WebGPU.CommandEncoder* native;

    public nint Native => (nint)native;

    public GPUCommandEncoder(IWGPUContext context, ReadOnlySpan<char> label)
    {
        this.context = context;
        var descriptor = new Silk.NET.WebGPU.CommandEncoderDescriptor(label: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(label)));
        native = context.wgpu.DeviceCreateCommandEncoder(context.Device, descriptor);
    }

    public void Dispose()
    {
        context.wgpu.CommandEncoderRelease(native);
    }

    public GPUCommandBuffer Finish(ReadOnlySpan<char> label)
    {
        var descriptor = new Silk.NET.WebGPU.CommandBufferDescriptor(label: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(label)));
        var commandBuffer = context.wgpu.CommandEncoderFinish(native, descriptor);
        return new GPUCommandBuffer(context, commandBuffer);
    }

    public GPURenderPassEncoder BeginRenderPass(RenderPassDescriptor descriptor)
    {
#if BROWSER
        Emscripten.WGPURenderPassColorAttachment_Browser* colorAttachments = 
            stackalloc Emscripten.WGPURenderPassColorAttachment_Browser[descriptor.ColorAttachments.Length];
        for (int i = 0; i < descriptor.ColorAttachments.Length; i++)
        {
            colorAttachments[i] = new Emscripten.WGPURenderPassColorAttachment_Browser
            {
                View = (Silk.NET.WebGPU.TextureView*)descriptor.ColorAttachments[i].View,
                ResolveTarget = (Silk.NET.WebGPU.TextureView*)descriptor.ColorAttachments[i].ResolveTarget,
                LoadOp = descriptor.ColorAttachments[i].LoadOp,
                StoreOp = descriptor.ColorAttachments[i].StoreOp,
                ClearValue = descriptor.ColorAttachments[i].ClearValue,
            };
        }
        var wgpuDescriptor = new Emscripten.WGPURenderPassDescriptor_Browser
        {
            Label = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Label)),
            ColorAttachmentCount = (uint)descriptor.ColorAttachments.Length,
            ColorAttachments = colorAttachments
        };
#else
        var wgpuDescriptor = new Silk.NET.WebGPU.RenderPassDescriptor
        {
            Label = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Label)),
            ColorAttachmentCount = (uint)descriptor.ColorAttachments.Length,
            ColorAttachments = (Silk.NET.WebGPU.RenderPassColorAttachment*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.ColorAttachments))
        };
#endif

        return new GPURenderPassEncoder(context, native, wgpuDescriptor);
    }
}
