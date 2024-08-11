namespace Pollus.Graphics.Rendering;

using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

unsafe public class GPUBindGroup : GPUResourceWrapper
{
    Silk.NET.WebGPU.BindGroup* native;

    public nint Native => (nint)native;

    public GPUBindGroup(IWGPUContext context, BindGroupDescriptor descriptor) : base(context)
    {
        using var pins = new TemporaryPins();
        pins.Pin(descriptor.Label);

        var nativeDescriptor = new Silk.NET.WebGPU.BindGroupDescriptor(
            label: (byte*)MemoryMarshal.AsBytes(descriptor.Label.AsSpan())[0],
            layout: (Silk.NET.WebGPU.BindGroupLayout*)descriptor.Layout.Native
        );

        if (descriptor.Entries is BindGroupEntry[] bindGroupEntries)
        {
            nativeDescriptor.EntryCount = (uint)bindGroupEntries.Length;
            var entries = new Silk.NET.WebGPU.BindGroupEntry[bindGroupEntries.Length];

            for (int i = 0; i < bindGroupEntries.Length; i++)
            {
                var entry = bindGroupEntries[i];
                var silkEntry = new Silk.NET.WebGPU.BindGroupEntry();
                silkEntry.Binding = entry.Binding;
                if (entry.Buffer is GPUBuffer buffer)
                {
                    silkEntry.Buffer = (Silk.NET.WebGPU.Buffer*)buffer.Native;
                    silkEntry.Offset = entry.Offset;
                    silkEntry.Size = entry.Size;
                }
                if (entry.TextureView is GPUTextureView textureView)
                {
                    silkEntry.TextureView = (Silk.NET.WebGPU.TextureView*)textureView.Native;
                }
                if (entry.Sampler is GPUSampler sampler)
                {
                    silkEntry.Sampler = (Silk.NET.WebGPU.Sampler*)sampler.Native;
                }

                entries[i] = silkEntry;
            }

            nativeDescriptor.Entries = (Silk.NET.WebGPU.BindGroupEntry*)pins.Pin(entries).AddrOfPinnedObject();
        }

        native = context.wgpu.DeviceCreateBindGroup(context.Device, nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.BindGroupRelease(native);
    }
}