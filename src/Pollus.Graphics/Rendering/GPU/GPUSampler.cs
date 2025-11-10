namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public class GPUSampler : GPUResourceWrapper
{
    NativeHandle<SamplerTag> native;

    public NativeHandle<SamplerTag> Native => native;

    public GPUSampler(IWGPUContext context, SamplerDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);
        native = context.Backend.DeviceCreateSampler(context.DeviceHandle, in descriptor, labelData);
    }

    protected override void Free()
    {
        context.Backend.SamplerRelease(native);
    }
}
