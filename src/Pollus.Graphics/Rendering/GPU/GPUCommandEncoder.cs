namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;
using Pollus.Mathematics;

unsafe public struct GPUCommandEncoder : IDisposable
{
    string label;
    IWGPUContext context;
    NativeHandle<CommandEncoderTag> native;

    public NativeHandle<CommandEncoderTag> Native => native;
    public string Label => label;

    public GPUCommandEncoder(IWGPUContext context, string label)
    {
        this.label = label;
        this.context = context;
        using var labelData = new NativeUtf8(label);
        native = context.Backend.DeviceCreateCommandEncoder(context.DeviceHandle, new Utf8Name((nint)labelData.Pointer));
    }

    public void Dispose()
    {
        context.Backend.CommandEncoderRelease(native);
    }

    public GPUCommandBuffer Finish(ReadOnlySpan<char> label)
    {
        using var labelData = new NativeUtf8(label);
        var buffer = context.Backend.CommandEncoderFinish(native, new Utf8Name((nint)labelData.Pointer));
        return new GPUCommandBuffer(context, buffer);
    }

    public GPURenderPassEncoder BeginRenderPass(RenderPassDescriptor descriptor)
    {
        return new GPURenderPassEncoder(context, this, descriptor);
    }

    public GPUComputePassEncoder BeginComputePass(string label)
    {
        return new GPUComputePassEncoder(context, this, label);
    }

    public void CopyTextureToTexture(GPUTexture srcTex, GPUTexture dstTex, Extent3D copySize)
    {
        context.Backend.CommandEncoderCopyTextureToTexture(
            native,
            srcTex.Native,
            dstTex.Native,
            copySize.Width, copySize.Height, copySize.DepthOrArrayLayers
        );
    }
}
