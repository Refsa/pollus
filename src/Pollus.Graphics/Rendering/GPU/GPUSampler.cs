namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;

unsafe public class GPUSampler : GPUResourceWrapper
{
    Silk.NET.WebGPU.Sampler* native;

    public nint Native => (nint)native;

    public GPUSampler(IWGPUContext context, SamplerDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);

        var nativeDescriptor = new Silk.NET.WebGPU.SamplerDescriptor(
            label: labelData.Pointer,
            addressModeU: descriptor.AddressModeU,
            addressModeV: descriptor.AddressModeV,
            addressModeW: descriptor.AddressModeW,
            magFilter: descriptor.MagFilter,
            minFilter: descriptor.MinFilter,
            mipmapFilter: descriptor.MipmapFilter,
            lodMinClamp: descriptor.LodMinClamp,
            lodMaxClamp: descriptor.LodMaxClamp,
            maxAnisotropy: descriptor.MaxAnisotropy
        );
        native = context.wgpu.DeviceCreateSampler(context.Device, in nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.SamplerRelease(native);
    }
}
