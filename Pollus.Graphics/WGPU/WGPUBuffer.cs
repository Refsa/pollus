using System.Runtime.InteropServices;

namespace Pollus.Graphics.WGPU;

unsafe public class WGPUBuffer : WGPUResourceWrapper
{
    Silk.NET.WebGPU.Buffer* native;
    ulong size;

    public nint Native => (nint)native;
    public ulong Size => size;

    public WGPUBuffer(WGPUContext context, WGPUBufferDescriptor descriptor) : base(context)
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

            native = context.wgpu.DeviceCreateBuffer(context.device, nativeDescriptor);
        }
    }

    protected override void Free()
    {
        context.wgpu.BufferRelease(native);
    }
}
