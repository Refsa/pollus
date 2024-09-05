namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;

unsafe public class GPUBuffer : GPUResourceWrapper
{
    NativeUtf8 label;
    Silk.NET.WebGPU.Buffer* native;
    ulong size;

    public nint Native => (nint)native;
    public ulong Size => size;

    public GPUBuffer(IWGPUContext context, BufferDescriptor descriptor) : base(context)
    {
        label = new NativeUtf8(descriptor.Label);
        size = descriptor.Size;

        var nativeDescriptor = new Silk.NET.WebGPU.BufferDescriptor(
            label: label.Pointer,
            usage: (Silk.NET.WebGPU.BufferUsage)descriptor.Usage,
            size: descriptor.Size,
            mappedAtCreation: descriptor.MappedAtCreation
        );

        native = context.wgpu.DeviceCreateBuffer(context.Device, nativeDescriptor);
    }

    public void Write(ReadOnlySpan<byte> data, int offset)
    {
        fixed (byte* ptr = data)
        {
            context.wgpu.QueueWriteBuffer(context.Queue, native, (nuint)offset, ptr, (nuint)data.Length);
        }
    }

    public void Write<TElement>(ReadOnlySpan<TElement> data, int offset = 0)
        where TElement : unmanaged, IShaderType
    {
        var alignedSize = Alignment.AlignedSize<TElement>((uint)data.Length);
        Write(data, alignedSize, offset);
    }

    public void Write<TElement>(ReadOnlySpan<TElement> data, uint alignedSize, int offset = 0)
        where TElement : unmanaged
    {
        fixed (TElement* ptr = data)
        {
            context.wgpu.QueueWriteBuffer(context.Queue, native, (nuint)offset, ptr, alignedSize);
        }
    }

    public void Write<TElement>(in TElement element, int offset)
        where TElement : unmanaged, IShaderType
    {
        var alignedSize = Alignment.AlignedSize<TElement>(1);
        Write(element, alignedSize, offset);
    }

    public void Write<TElement>(in TElement element, uint alignedSize, int offset)
        where TElement : unmanaged
    {
        fixed (TElement* ptr = &element)
        {
            context.wgpu.QueueWriteBuffer(context.Queue, native, (nuint)offset, ptr, alignedSize);
        }
    }

    public void Resize<TElement>(uint newCapacity)
        where TElement : unmanaged, IShaderType
    {
        size = Alignment.AlignedSize<TElement>(newCapacity);

        var newBuffer = context.wgpu.DeviceCreateBuffer(context.Device, new Silk.NET.WebGPU.BufferDescriptor
        {
            Label = label.Pointer,
            Size = size,
            Usage = Silk.NET.WebGPU.BufferUsage.CopyDst | Silk.NET.WebGPU.BufferUsage.Vertex,
        });

        context.wgpu.BufferDestroy(native);
        context.wgpu.BufferRelease(native);
        native = newBuffer;
    }

    protected override void Free()
    {
        context.wgpu.BufferDestroy(native);
        context.wgpu.BufferRelease(native);
        label.Dispose();
    }
}
