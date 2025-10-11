namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

unsafe public class GPUTexture : GPUResourceWrapper
{
    Silk.NET.WebGPU.Texture* texture;
    TextureDescriptor descriptor;

    public nint Native => (nint)texture;
    public ref readonly TextureDescriptor Descriptor => ref descriptor;

    public GPUTexture(IWGPUContext context, TextureDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);

        var nativeDescriptor = new Silk.NET.WebGPU.TextureDescriptor(
            label: labelData.Pointer,
            usage: (Silk.NET.WebGPU.TextureUsage)descriptor.Usage,
            dimension: (Silk.NET.WebGPU.TextureDimension)descriptor.Dimension,
            size: descriptor.Size,
            format: (Silk.NET.WebGPU.TextureFormat)descriptor.Format,
            mipLevelCount: descriptor.MipLevelCount,
            sampleCount: descriptor.SampleCount
        );

        int viewFormatCount = 0;
        foreach (var viewFormat in descriptor.ViewFormats)
        {
            if (viewFormat == TextureFormat.Undefined) break;
            viewFormatCount++;
        }
        var viewFormats = stackalloc Silk.NET.WebGPU.TextureFormat[viewFormatCount];
        for (int i = 0; i < viewFormatCount; i++) viewFormats[i] = (Silk.NET.WebGPU.TextureFormat)descriptor.ViewFormats[i];
        nativeDescriptor.ViewFormatCount = (nuint)viewFormatCount;
        nativeDescriptor.ViewFormats = viewFormats;

        this.descriptor = descriptor;
        texture = context.wgpu.DeviceCreateTexture(context.Device, in nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.TextureDestroy(texture);
        context.wgpu.TextureRelease(texture);
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
        var destination = new Silk.NET.WebGPU.ImageCopyTexture(
            texture: texture,
            mipLevel: mipLevel,
            origin: new Silk.NET.WebGPU.Origin3D(
                x: origin.X,
                y: origin.Y,
                z: origin.Z
            )
        );

        var layout = new Silk.NET.WebGPU.TextureDataLayout(
            offset: 0,
            bytesPerRow: descriptor.Format.BytesPerPixel() * descriptor.Size.Width,
            rowsPerImage: descriptor.Size.Height
        );

        var writeSize = size switch
        {
            { X: var x, Y: var y, Z: var z } => new Extent3D(
                width: x,
                height: y,
                depthOrArrayLayers: z
            ),
            _ => descriptor.Size,
        };

#pragma warning disable CS9192
        context.wgpu.QueueWriteTexture(context.Queue, destination,
            Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)),
            (nuint)data.Length,
            layout, writeSize
        );
#pragma warning restore CS9192
    }
}