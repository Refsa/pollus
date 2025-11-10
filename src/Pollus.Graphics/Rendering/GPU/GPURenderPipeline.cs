namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Collections;
using Pollus.Graphics.Platform;

unsafe public class GPURenderPipeline : GPUResourceWrapper
{
    NativeHandle<RenderPipelineTag> native;

    public NativeHandle<RenderPipelineTag> Native => native;

    public GPURenderPipeline(IWGPUContext context, RenderPipelineDescriptor descriptor) : base(context)
    {
        using var labelUtf8 = new NativeUtf8(descriptor.Label);
        native = context.Backend.DeviceCreateRenderPipeline(context.DeviceHandle, in descriptor, labelUtf8);
    }

    protected override void Free()
    {
        context.Backend.RenderPipelineRelease(native);
    }
}