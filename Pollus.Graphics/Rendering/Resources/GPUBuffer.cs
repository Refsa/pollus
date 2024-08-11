using System.Runtime.CompilerServices;
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

    public void Write<TElement>(ReadOnlySpan<TElement> data)
        where TElement : unmanaged
    {
        fixed (TElement* ptr = data)
        {
            context.wgpu.QueueWriteBuffer(context.Queue, native, 0, ptr, (nuint)(data.Length * Unsafe.SizeOf<TElement>()));
        }
    }

    protected override void Free()
    {
        context.wgpu.BufferRelease(native);
    }
}
