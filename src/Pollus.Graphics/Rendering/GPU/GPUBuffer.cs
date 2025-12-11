namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;
using System.Runtime.InteropServices;

unsafe public class GPUBuffer : GPUResourceWrapper
{
    NativeUtf8 label;
    BufferDescriptor descriptor;
    NativeHandle<BufferTag> native;
    ulong size;

    public NativeHandle<BufferTag> Native => native;
    public ulong Size => size;

    public GPUBuffer(IWGPUContext context, BufferDescriptor descriptor) : base(context)
    {
        label = new NativeUtf8(descriptor.Label);
        size = descriptor.Size;
        this.descriptor = descriptor;

        native = context.Backend.DeviceCreateBuffer(context.DeviceHandle, in descriptor, label);
    }

    protected override void Free()
    {
        context.Backend.BufferDestroy(native);
        context.Backend.BufferRelease(native);
        label.Dispose();
    }

    public void Write(ReadOnlySpan<byte> data, int offset)
    {
        context.Backend.QueueWriteBuffer(context.QueueHandle, native, (nuint)offset, data, (uint)data.Length);
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
        context.Backend.QueueWriteBuffer(context.QueueHandle, native, (nuint)offset, MemoryMarshal.Cast<TElement, byte>(data), alignedSize);
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
        ReadOnlySpan<TElement> span = stackalloc TElement[] { element };
        context.Backend.QueueWriteBuffer(context.QueueHandle, native, (nuint)offset, MemoryMarshal.Cast<TElement, byte>(span), alignedSize);
    }

    public void Resize<TElement>(uint newCapacity)
        where TElement : unmanaged, IShaderType
    {
        var newSize = Alignment.AlignedSize<TElement>(newCapacity);
        Resize(newSize);
    }

    public void Resize(uint newSize)
    {
        if (newSize == 0 || newSize <= size) return;
        size = newSize;

        var updated = descriptor;
        updated.Size = size;
        var newBuffer = context.Backend.DeviceCreateBuffer(context.DeviceHandle, in updated, label);
        context.Backend.BufferDestroy(native);
        context.Backend.BufferRelease(native);
        native = newBuffer;
    }
}
