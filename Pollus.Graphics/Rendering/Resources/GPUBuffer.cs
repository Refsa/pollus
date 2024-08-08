using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;

namespace Pollus.Graphics.Rendering;

unsafe public class GPUBuffer : GPUResourceWrapper
{
    Silk.NET.WebGPU.Buffer* native;
    ulong size;

    public nint Native => (nint)native;
    public ulong Size => size;

    public GPUBuffer(IWGPUContext context, BufferDescriptor descriptor) : base(context)
    {
        size = descriptor.Size;

        var labelSpan = MemoryMarshal.AsBytes(descriptor.Label.AsSpan());

        fixed (byte* labelPtr = labelSpan)
        {
            var nativeDescriptor = new Silk.NET.WebGPU.BufferDescriptor(
                label: labelPtr,
                usage: descriptor.Usage,
                size: descriptor.Size,
                mappedAtCreation: descriptor.MappedAtCreation
            );

            native = context.wgpu.DeviceCreateBuffer(context.Device, nativeDescriptor);
        }
    }

    protected override void Free()
    {
        context.wgpu.BufferRelease(native);
    }
}
