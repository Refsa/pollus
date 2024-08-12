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

    Silk.NET.WebGPU.TextureDimension dimension;
    Silk.NET.WebGPU.Extent3D size;
    Silk.NET.WebGPU.TextureFormat format;
    uint mipLevelCount;
    uint sampleCount;

    public nint Native => (nint)texture;

    public GPUTexture(IWGPUContext context, TextureDescriptor descriptor) : base(context)
    {
        using var labelData = new NativeUtf8(descriptor.Label);

        var nativeDescriptor = new Silk.NET.WebGPU.TextureDescriptor(
            label: labelData.Pointer,
            usage: descriptor.Usage,
            dimension: descriptor.Dimension,
            size: descriptor.Size,
            format: descriptor.Format,
            mipLevelCount: descriptor.MipLevelCount,
            sampleCount: descriptor.SampleCount
        );

        if (descriptor.ViewFormats.Length > 0)
        {
            throw new NotImplementedException("ViewFormats for GPUTexture is not implemented yet.");
        }

        size = descriptor.Size;
        format = descriptor.Format;
        dimension = descriptor.Dimension;
        mipLevelCount = descriptor.MipLevelCount;
        sampleCount = descriptor.SampleCount;
        texture = context.wgpu.DeviceCreateTexture(context.Device, nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.TextureDestroy(texture);
        context.wgpu.TextureRelease(texture);
    }

    public GPUTextureView GetTextureView()
    {
        var view = context.wgpu.TextureCreateView(texture, null);
        return new GPUTextureView(context, view);
    }

    public void Write<T>(ReadOnlySpan<T> data, uint mipLevel = 0, Vec3<uint> origin = default, Vec3<uint>? size = null)
        where T : unmanaged
    {
        Write(MemoryMarshal.AsBytes(data), mipLevel, origin, size);
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
            bytesPerRow: GetBytesPerPixel() * this.size.Width,
            rowsPerImage: this.size.Height
        );

        var writeSize = size switch
        {
            { X: var x, Y: var y, Z: var z } => new Silk.NET.WebGPU.Extent3D(
                width: x,
                height: y,
                depthOrArrayLayers: z
            ),
            _ => this.size,
        };

        context.wgpu.QueueWriteTexture(context.Queue, destination,
            Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)),
            (nuint)data.Length,
            layout, writeSize
        );
    }

    private uint GetBytesPerPixel()
    {
        return format switch
        {
            // 1 byte
            Silk.NET.WebGPU.TextureFormat.R8Unorm => 1,
            Silk.NET.WebGPU.TextureFormat.R8Snorm => 1,
            Silk.NET.WebGPU.TextureFormat.R8Uint => 1,
            Silk.NET.WebGPU.TextureFormat.R8Sint => 1,
            // 2 bytes
            Silk.NET.WebGPU.TextureFormat.R16Uint => 2,
            Silk.NET.WebGPU.TextureFormat.R16Sint => 2,
            Silk.NET.WebGPU.TextureFormat.R16float => 2,
            Silk.NET.WebGPU.TextureFormat.RG8Unorm => 2,
            Silk.NET.WebGPU.TextureFormat.RG8Snorm => 2,
            Silk.NET.WebGPU.TextureFormat.RG8Uint => 2,
            Silk.NET.WebGPU.TextureFormat.RG8Sint => 2,

            // 4 bytes
            Silk.NET.WebGPU.TextureFormat.R32Uint => 4,
            Silk.NET.WebGPU.TextureFormat.R32Sint => 4,
            Silk.NET.WebGPU.TextureFormat.R32float => 4,
            Silk.NET.WebGPU.TextureFormat.RG16Uint => 3,
            Silk.NET.WebGPU.TextureFormat.RG16Sint => 3,
            Silk.NET.WebGPU.TextureFormat.RG16float => 3,
            Silk.NET.WebGPU.TextureFormat.Rgba8Unorm => 4,
            Silk.NET.WebGPU.TextureFormat.Rgba8UnormSrgb => 4,
            Silk.NET.WebGPU.TextureFormat.Rgba8Snorm => 4,
            Silk.NET.WebGPU.TextureFormat.Rgba8Uint => 4,
            Silk.NET.WebGPU.TextureFormat.Rgba8Sint => 4,
            Silk.NET.WebGPU.TextureFormat.Bgra8Unorm => 4,
            Silk.NET.WebGPU.TextureFormat.Bgra8UnormSrgb => 4,
            
            // Packed 4 bytes
            Silk.NET.WebGPU.TextureFormat.Rgb9E5Ufloat => 4,
            Silk.NET.WebGPU.TextureFormat.RG11B10Ufloat => 4,
            Silk.NET.WebGPU.TextureFormat.Rgb10A2Unorm => 4,

            // 8 bytes
            Silk.NET.WebGPU.TextureFormat.RG32Uint => 8,
            Silk.NET.WebGPU.TextureFormat.RG32Sint => 8,
            Silk.NET.WebGPU.TextureFormat.RG32float => 8,
            Silk.NET.WebGPU.TextureFormat.Rgba16Uint => 8,
            Silk.NET.WebGPU.TextureFormat.Rgba16Sint => 8,
            Silk.NET.WebGPU.TextureFormat.Rgba16float => 8,

            // 16 bytes
            Silk.NET.WebGPU.TextureFormat.Rgba32Uint => 16,
            Silk.NET.WebGPU.TextureFormat.Rgba32Sint => 16,
            Silk.NET.WebGPU.TextureFormat.Rgba32float => 16,

            // Depth/Stencil
            Silk.NET.WebGPU.TextureFormat.Stencil8 => 1,
            Silk.NET.WebGPU.TextureFormat.Depth16Unorm => 2,
            Silk.NET.WebGPU.TextureFormat.Depth32float => 4,
            Silk.NET.WebGPU.TextureFormat.Depth24Plus => 4,
            Silk.NET.WebGPU.TextureFormat.Depth24PlusStencil8 => 4,
            _ => throw new IndexOutOfRangeException($"Unknown texture format: {format}")
        };
    }
}