namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

public class GPUBindGroup : GPUResourceWrapper
{
    NativeHandle<BindGroupTag> native;

    public NativeHandle<BindGroupTag> Native => native;

    public GPUBindGroup(IWGPUContext context, BindGroupDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);

        native = context.Backend.DeviceCreateBindGroup(context.DeviceHandle, in descriptor, labelData);
    }

    protected override void Free()
    {
        context.Backend.BindGroupRelease(native);
        native = NativeHandle<BindGroupTag>.Null;
    }
}
