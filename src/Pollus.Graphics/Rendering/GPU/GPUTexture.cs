namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
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
        texture = context.wgpu.DeviceCreateTexture(context.Device, nativeDescriptor);
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
            bytesPerRow: GetBytesPerPixel() * descriptor.Size.Width,
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

        context.wgpu.QueueWriteTexture(context.Queue, destination,
            Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)),
            (nuint)data.Length,
            layout, writeSize
        );
    }

    private uint GetBytesPerPixel()
    {
        return descriptor.Format switch
        {
            // 1 byte
            TextureFormat.R8Unorm => 1,
            TextureFormat.R8Snorm => 1,
            TextureFormat.R8Uint => 1,
            TextureFormat.R8Sint => 1,
            // 2 bytes
            TextureFormat.R16Uint => 2,
            TextureFormat.R16Sint => 2,
            TextureFormat.R16float => 2,
            TextureFormat.RG8Unorm => 2,
            TextureFormat.RG8Snorm => 2,
            TextureFormat.RG8Uint => 2,
            TextureFormat.RG8Sint => 2,

            // 4 bytes
            TextureFormat.R32Uint => 4,
            TextureFormat.R32Sint => 4,
            TextureFormat.R32float => 4,
            TextureFormat.RG16Uint => 3,
            TextureFormat.RG16Sint => 3,
            TextureFormat.RG16float => 3,
            TextureFormat.Rgba8Unorm => 4,
            TextureFormat.Rgba8UnormSrgb => 4,
            TextureFormat.Rgba8Snorm => 4,
            TextureFormat.Rgba8Uint => 4,
            TextureFormat.Rgba8Sint => 4,
            TextureFormat.Bgra8Unorm => 4,
            TextureFormat.Bgra8UnormSrgb => 4,

            // Packed 4 bytes
            TextureFormat.Rgb9E5Ufloat => 4,
            TextureFormat.RG11B10Ufloat => 4,
            TextureFormat.Rgb10A2Unorm => 4,

            // 8 bytes
            TextureFormat.RG32Uint => 8,
            TextureFormat.RG32Sint => 8,
            TextureFormat.RG32float => 8,
            TextureFormat.Rgba16Uint => 8,
            TextureFormat.Rgba16Sint => 8,
            TextureFormat.Rgba16float => 8,

            // 16 bytes
            TextureFormat.Rgba32Uint => 16,
            TextureFormat.Rgba32Sint => 16,
            TextureFormat.Rgba32float => 16,

            // Depth/Stencil
            TextureFormat.Stencil8 => 1,
            TextureFormat.Depth16Unorm => 2,
            TextureFormat.Depth32float => 4,
            TextureFormat.Depth24Plus => 4,
            TextureFormat.Depth24PlusStencil8 => 4,
            _ => throw new IndexOutOfRangeException($"Unknown texture format: {descriptor.Format}")
        };
    }
}