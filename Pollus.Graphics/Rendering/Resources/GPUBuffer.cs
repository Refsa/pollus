namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

unsafe public class GPUBuffer : GPUResourceWrapper
{
    Silk.NET.WebGPU.Buffer* native;
    ulong size;

    public nint Native => (nint)native;
    public ulong Size => size;

    public GPUBuffer(IWGPUContext context, BufferDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);
        size = descriptor.Size;

        var nativeDescriptor = new Silk.NET.WebGPU.BufferDescriptor(
            label: labelData.Pointer,
            usage: descriptor.Usage,
            size: descriptor.Size,
            mappedAtCreation: descriptor.MappedAtCreation
        );

        native = context.wgpu.DeviceCreateBuffer(context.Device, nativeDescriptor);
    }

    public void Write<TElement>(ReadOnlySpan<TElement> data, int offset = 0)
        where TElement : unmanaged
    {
        fixed (TElement* ptr = data)
        {
            context.wgpu.QueueWriteBuffer(context.Queue, native, (nuint)offset, ptr, (nuint)(data.Length * Unsafe.SizeOf<TElement>()));
        }
    }

    public void Write<TElement>(in TElement element, int offset)
        where TElement : unmanaged
    {
        fixed (TElement* ptr = &element)
        {
            context.wgpu.QueueWriteBuffer(context.Queue, native, (nuint)offset, ptr, (nuint)Unsafe.SizeOf<TElement>());
        }
    }

    public void WriteAligned<TElement>(in TElement element, int offset)
        where TElement : unmanaged
    {
        fixed (TElement* ptr = &element)
        {
            context.wgpu.QueueWriteBuffer(context.Queue, native, (nuint)offset, ptr, Alignment.GetAlignedSize<TElement>());
        }
    }

    protected override void Free()
    {
        context.wgpu.BufferRelease(native);
    }
}
