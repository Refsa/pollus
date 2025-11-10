namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;
using Pollus.Mathematics;

unsafe public class GPUTexture : GPUResourceWrapper
{
    NativeHandle<TextureTag> native;
    TextureDescriptor descriptor;

    public NativeHandle<TextureTag> Native => native;
    public ref readonly TextureDescriptor Descriptor => ref descriptor;

    public GPUTexture(IWGPUContext context, TextureDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);
        this.descriptor = descriptor;
        native = context.Backend.DeviceCreateTexture(context.DeviceHandle, in descriptor, labelData);
    }

    protected override void Free()
    {
        context.Backend.TextureDestroy(native);
        context.Backend.TextureRelease(native);
    }

    public GPUTextureView GetTextureView()
    {
        return new GPUTextureView(context, this);
    }

    public void Write<T>(ReadOnlySpan<T> data, uint mipLevel = 0, Vec3<uint> origin = default, Vec3<uint>? size = null)
        where T : unmanaged
    {
        Write(MemoryMarshal.AsBytes(data), mipLevel, origin, size);
    }

    public void Write(nint bytes, int byteCount, uint mipLevel = 0, Vec3<uint> origin = default, Vec3<uint>? size = null)
    {
        Write(new ReadOnlySpan<byte>((void*)bytes, byteCount), mipLevel, origin, size);
    }

    public void Write(ReadOnlySpan<byte> data, uint mipLevel = 0, Vec3<uint> origin = default, Vec3<uint>? size = null)
    {
        var bytesPerRow = descriptor.Format.BytesPerPixel() * descriptor.Size.Width;
        var rowsPerImage = descriptor.Size.Height;
        var writeWidth = size?.X ?? descriptor.Size.Width;
        var writeHeight = size?.Y ?? descriptor.Size.Height;
        var writeDepth = size?.Z ?? descriptor.Size.DepthOrArrayLayers;
        context.Backend.QueueWriteTexture(
            context.QueueHandle,
            native,
            mipLevel,
            origin.X, origin.Y, origin.Z,
            data,
            bytesPerRow,
            rowsPerImage,
            writeWidth, writeHeight, writeDepth
        );
    }
}