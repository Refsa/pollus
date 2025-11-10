namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public class GPUBindGroupLayout : GPUResourceWrapper
{
    nint native;

    public nint Native => native;

    public GPUBindGroupLayout(IWGPUContext context, BindGroupLayoutDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);
        var handle = context.Backend.DeviceCreateBindGroupLayout(context.DeviceHandle, in descriptor, labelData);
        native = handle.Ptr;
    }

    protected override void Free()
    {
        context.Backend.BindGroupLayoutRelease(new NativeHandle<BindGroupLayoutTag>(native));
    }
}
