namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

public readonly struct GPUCommandEncoder : IDisposable
{
    readonly IWGPUContext context;
    readonly NativeHandle<CommandEncoderTag> native;

    public NativeHandle<CommandEncoderTag> Native => native;

    public GPUCommandEncoder(IWGPUContext context, ReadOnlySpan<char> label)
    {
        this.context = context;
        using var labelPtr = new NativeUtf8(label);
        native = context.Backend.DeviceCreateCommandEncoder(context.DeviceHandle, labelPtr);
    }

    public void Dispose()
    {
        context.Backend.CommandEncoderRelease(native);
    }

    public GPUCommandBuffer Finish(ReadOnlySpan<char> label)
    {
        using var labelPtr = new NativeUtf8(label);
        var buffer = context.Backend.CommandEncoderFinish(native, labelPtr);
        return new GPUCommandBuffer(context, buffer);
    }

    public GPURenderPassEncoder BeginRenderPass(in RenderPassDescriptor descriptor)
    {
        return new GPURenderPassEncoder(context, this, descriptor);
    }

    public GPUComputePassEncoder BeginComputePass(ReadOnlySpan<char> label)
    {
        return new GPUComputePassEncoder(context, this, label);
    }

    public void InsertDebugMarker(ReadOnlySpan<char> label)
    {
        using var labelPtr = new NativeUtf8(label);
        context.Backend.CommandEncoderInsertDebugMarker(native, labelPtr);
    }

    public void PushDebugGroup(ReadOnlySpan<char> label)
    {
        using var labelPtr = new NativeUtf8(label);
        context.Backend.CommandEncoderPushDebugGroup(native, labelPtr);
    }

    public void PopDebugGroup()
    {
        context.Backend.CommandEncoderPopDebugGroup(native);
    }

    public DebugGroupScopeHandler DebugGroupScope(ReadOnlySpan<char> label)
    {
        return new DebugGroupScopeHandler(this, label);
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

    public ref struct DebugGroupScopeHandler : IDisposable
    {
        GPUCommandEncoder encoder;

        public DebugGroupScopeHandler(GPUCommandEncoder encoder, ReadOnlySpan<char> label)
        {
            this.encoder = encoder;
            encoder.PushDebugGroup(label);
        }

        public void Dispose()
        {
            encoder.PopDebugGroup();
        }
    }
}
