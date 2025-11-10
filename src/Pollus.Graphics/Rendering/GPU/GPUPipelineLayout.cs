namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public class GPUPipelineLayout : GPUResourceWrapper
{
    NativeHandle<PipelineLayoutTag> native;

    public NativeHandle<PipelineLayoutTag> Native => native;

    public GPUPipelineLayout(IWGPUContext context, PipelineLayoutDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);
        native = context.Backend.DeviceCreatePipelineLayout(context.DeviceHandle, in descriptor, labelData);
    }

    protected override void Free()
    {
        context.Backend.PipelineLayoutRelease(native);
    }
}
