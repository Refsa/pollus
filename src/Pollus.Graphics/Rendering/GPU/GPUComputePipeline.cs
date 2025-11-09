namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public class GPUComputePipeline : GPUResourceWrapper
{
    NativeHandle<ComputePipelineTag> native;
    public NativeHandle<ComputePipelineTag> Native => native;

    public GPUComputePipeline(IWGPUContext context, ComputePipelineDescriptor descriptor) : base(context)
    {
        using var label = new NativeUtf8(descriptor.Label);
        native = context.Backend.DeviceCreateComputePipeline(context.DeviceHandle, in descriptor, new Utf8Name((nint)label.Pointer));
    }

    protected override void Free()
    {
        context.Backend.ComputePipelineRelease(native);
    }
}