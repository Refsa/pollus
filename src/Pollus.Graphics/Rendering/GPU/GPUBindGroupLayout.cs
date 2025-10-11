namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Pollus.Collections;
using Pollus.Graphics.WGPU;

unsafe public class GPUBindGroupLayout : GPUResourceWrapper
{
    Silk.NET.WebGPU.BindGroupLayout* native;

    public nint Native => (nint)native;

    public GPUBindGroupLayout(IWGPUContext context, BindGroupLayoutDescriptor descriptor) : base(context)
    {
        this.context = context;

        var entries = new Silk.NET.WebGPU.BindGroupLayoutEntry[descriptor.Entries.Length];
        for (int i = 0; i < descriptor.Entries.Length; i++)
        {
            entries[i] = new Silk.NET.WebGPU.BindGroupLayoutEntry(
                binding: descriptor.Entries[i].Binding,
                visibility: (Silk.NET.WebGPU.ShaderStage)descriptor.Entries[i].Visibility,
                buffer: new Silk.NET.WebGPU.BufferBindingLayout(
                    type: (Silk.NET.WebGPU.BufferBindingType)descriptor.Entries[i].Buffer.Type,
                    minBindingSize: descriptor.Entries[i].Buffer.MinBindingSize,
                    hasDynamicOffset: descriptor.Entries[i].Buffer.HasDynamicOffset),
                sampler: new Silk.NET.WebGPU.SamplerBindingLayout(
                    type: (Silk.NET.WebGPU.SamplerBindingType)descriptor.Entries[i].Sampler.Type
                ),
                texture: new Silk.NET.WebGPU.TextureBindingLayout(
                    sampleType: (Silk.NET.WebGPU.TextureSampleType)descriptor.Entries[i].Texture.SampleType,
                    viewDimension: (Silk.NET.WebGPU.TextureViewDimension)descriptor.Entries[i].Texture.ViewDimension,
                    multisampled: descriptor.Entries[i].Texture.Multisampled),
                storageTexture: new Silk.NET.WebGPU.StorageTextureBindingLayout(
                    access: (Silk.NET.WebGPU.StorageTextureAccess)descriptor.Entries[i].StorageTexture.Access,
                    format: (Silk.NET.WebGPU.TextureFormat)descriptor.Entries[i].StorageTexture.Format,
                    viewDimension: (Silk.NET.WebGPU.TextureViewDimension)descriptor.Entries[i].StorageTexture.ViewDimension
                )
            );
        }

        using var labelData = new NativeUtf8(descriptor.Label);

        var entriesSpan = entries.AsSpan();
        fixed (Silk.NET.WebGPU.BindGroupLayoutEntry* entriesPtr = entriesSpan)
        {
            var nativeDescriptor = new Silk.NET.WebGPU.BindGroupLayoutDescriptor(
                label: labelData.Pointer,
                entryCount: (uint)entries.Length,
                entries: entriesPtr
            );

            native = context.wgpu.DeviceCreateBindGroupLayout(context.Device, in nativeDescriptor);
        }
    }

    protected override void Free()
    {
        context.wgpu.BindGroupLayoutRelease(native);
    }
}
