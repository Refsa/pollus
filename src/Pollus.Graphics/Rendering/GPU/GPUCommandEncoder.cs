namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

unsafe public struct GPUCommandEncoder : IDisposable
{
    string label;
    IWGPUContext context;
    Silk.NET.WebGPU.CommandEncoder* native;

    public nint Native => (nint)native;
    public string Label => label;

    public GPUCommandEncoder(IWGPUContext context, string label)
    {
        this.label = label;
        using var labelData = new NativeUtf8(label);

        this.context = context;
        var descriptor = new Silk.NET.WebGPU.CommandEncoderDescriptor(label: labelData.Pointer);
        native = context.wgpu.DeviceCreateCommandEncoder(context.Device, in descriptor);
    }

    public void Dispose()
    {
        context.wgpu.CommandEncoderRelease(native);
    }

    public GPUCommandBuffer Finish(ReadOnlySpan<char> label)
    {
        using var labelData = new NativeUtf8(label);

        var descriptor = new Silk.NET.WebGPU.CommandBufferDescriptor(label: labelData.Pointer);
        var commandBuffer = context.wgpu.CommandEncoderFinish(native, in descriptor);
        return new GPUCommandBuffer(context, commandBuffer);
    }

    public GPURenderPassEncoder BeginRenderPass(RenderPassDescriptor descriptor)
    {
#if BROWSER
        Emscripten.WGPU.WGPURenderPassColorAttachment* colorAttachments =
            stackalloc Emscripten.WGPU.WGPURenderPassColorAttachment[descriptor.ColorAttachments.Length];
        for (int i = 0; i < descriptor.ColorAttachments.Length; i++)
        {
            var clearValue = descriptor.ColorAttachments[i].ClearValue;
            colorAttachments[i] = new Emscripten.WGPU.WGPURenderPassColorAttachment
            {
                NextInChain = null,
                View = (Emscripten.WGPU.WGPUTextureView*)descriptor.ColorAttachments[i].View,
                DepthSlice = Emscripten.WGPU.WGPUBrowser.WGPU_DEPTH_SLICE_UNDEFINED,
                ResolveTarget = (Emscripten.WGPU.WGPUTextureView*)descriptor.ColorAttachments[i].ResolveTarget,
                LoadOp = (Emscripten.WGPU.WGPULoadOp)descriptor.ColorAttachments[i].LoadOp,
                StoreOp = (Emscripten.WGPU.WGPUStoreOp)descriptor.ColorAttachments[i].StoreOp,
                ClearValue = Unsafe.As<Vec4<double>, Emscripten.WGPU.WGPUColor>(ref clearValue),
            };
        }
        var wgpuDescriptor = new Emscripten.WGPU.WGPURenderPassDescriptor
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

    public GPUComputePassEncoder BeginComputePass(string label)
    {
        return new GPUComputePassEncoder(context, this, label);
    }

    public void CopyTextureToTexture(GPUTexture srcTex, GPUTexture dstTex, Extent3D copySize)
    {
        var silkExtents = new Silk.NET.WebGPU.Extent3D(copySize.Width, copySize.Height, copySize.DepthOrArrayLayers);
        Silk.NET.WebGPU.ImageCopyTexture src = new(
            texture: (Silk.NET.WebGPU.Texture*)srcTex.Native
        );
        Silk.NET.WebGPU.ImageCopyTexture dst = new(
            texture: (Silk.NET.WebGPU.Texture*)dstTex.Native
        );

        context.wgpu.CommandEncoderCopyTextureToTexture(native, in src, in dst, in silkExtents);
    }
}
