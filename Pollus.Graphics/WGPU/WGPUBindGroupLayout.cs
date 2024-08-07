namespace Pollus.Graphics.WGPU;

using System.Runtime.InteropServices;

unsafe public class WGPUBindGroupLayout : WGPUResourceWrapper
{
    Silk.NET.WebGPU.BindGroupLayout* native;

    public nint Native => (nint)native;

    public WGPUBindGroupLayout(IWGPUContext context, WGPUBindGroupLayoutDescriptor descriptor) : base(context)
    {
        this.context = context;

        var entries = new Silk.NET.WebGPU.BindGroupLayoutEntry[descriptor.Entries.Length];
        for (int i = 0; i < descriptor.Entries.Length; i++)
        {
            entries[i] = new Silk.NET.WebGPU.BindGroupLayoutEntry(
                binding: descriptor.Entries[i].Binding,
                visibility: descriptor.Entries[i].Visibility,
                buffer: new Silk.NET.WebGPU.BufferBindingLayout(
                    type: descriptor.Entries[i].Buffer.Type,
                    minBindingSize: descriptor.Entries[i].Buffer.MinBindingSize,
                    hasDynamicOffset: descriptor.Entries[i].Buffer.HasDynamicOffset),
                sampler: new Silk.NET.WebGPU.SamplerBindingLayout(
                    type: descriptor.Entries[i].Sampler.Type
                ),
                texture: new Silk.NET.WebGPU.TextureBindingLayout(
                    sampleType: descriptor.Entries[i].Texture.SampleType,
                    viewDimension: descriptor.Entries[i].Texture.ViewDimension,
                    multisampled: descriptor.Entries[i].Texture.Multisampled),
                storageTexture: new Silk.NET.WebGPU.StorageTextureBindingLayout(
                    access: descriptor.Entries[i].StorageTexture.Access,
                    format: descriptor.Entries[i].StorageTexture.Format,
                    viewDimension: descriptor.Entries[i].StorageTexture.ViewDimension
                )
            );
        }

        var labelSpan = MemoryMarshal.AsBytes(descriptor.Label.AsSpan());
        var entriesSpan = entries.AsSpan();

        fixed (byte* labelPtr = labelSpan)
        fixed (Silk.NET.WebGPU.BindGroupLayoutEntry* entriesPtr = entriesSpan)
        {
            var nativeDescriptor = new Silk.NET.WebGPU.BindGroupLayoutDescriptor(
                label: labelPtr,
                entryCount: (uint)entries.Length,
                entries: entriesPtr
            );

            native = context.wgpu.DeviceCreateBindGroupLayout(context.Device, nativeDescriptor);
        }
    }

    protected override void Free()
    {
        context.wgpu.BindGroupLayoutRelease(native);
    }
}
