namespace Pollus.Graphics.Rendering;

using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;

unsafe public class GPUSampler : GPUResourceWrapper
{
    Silk.NET.WebGPU.Sampler* native;

    public nint Native => (nint)native;

    public GPUSampler(IWGPUContext context, SamplerDescriptor descriptor) : base(context)
    {
        var labelSpan = MemoryMarshal.AsBytes(descriptor.Label.AsSpan());

        fixed (byte* labelPtr = labelSpan)
        {
            var nativeDescriptor = new Silk.NET.WebGPU.SamplerDescriptor(
                label: labelPtr,
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
            native = context.wgpu.DeviceCreateSampler(context.Device, nativeDescriptor);
        }
    }

    protected override void Free()
    {
        context.wgpu.SamplerRelease(native);
    }
}