namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public class GPUBindGroupLayout : GPUResourceWrapper
{
    NativeHandle<BindGroupLayoutTag> native;

    public NativeHandle<BindGroupLayoutTag> Native => native;

    public GPUBindGroupLayout(IWGPUContext context, BindGroupLayoutDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);
        native = context.Backend.DeviceCreateBindGroupLayout(context.DeviceHandle, in descriptor, labelData);
    }

    protected override void Free()
    {
        context.Backend.BindGroupLayoutRelease(native);
    }
}
