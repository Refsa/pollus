namespace Pollus.Graphics.Rendering;

public enum VertexFormat
{
    Undefined = 0,
    Uint8x2 = 1,
    Uint8x4 = 2,
    Sint8x2 = 3,
    Sint8x4 = 4,
    Unorm8x2 = 5,
    Unorm8x4 = 6,
    Snorm8x2 = 7,
    Snorm8x4 = 8,
    Uint16x2 = 9,
    Uint16x4 = 10,
    Sint16x2 = 11,
    Sint16x4 = 12,
    Unorm16x2 = 13,
    Unorm16x4 = 14,
    Snorm16x2 = 15,
    Snorm16x4 = 16,
    Float16x2 = 17,
    Float16x4 = 18,
    Float32 = 19,
    Float32x2 = 20,
    Float32x3 = 21,
    Float32x4 = 22,
    Uint32 = 23,
    Uint32x2 = 24,
    Uint32x3 = 25,
    Uint32x4 = 26,
    Sint32 = 27,
    Sint32x2 = 28,
    Sint32x3 = 29,
    Sint32x4 = 30,

    /// <summary>
    /// Not supported by rendering backend, is transformed to 3 x Float32x4
    /// </summary>
    Mat3x4 = int.MaxValue - 3,

    /// <summary>
    /// Not supported by rendering backend, is transformed to 3 x Float32x3
    /// </summary>
    Mat3x3 = int.MaxValue - 2,

    /// <summary>
    /// Not supported by rendering backend, is transformed to 4 x Float32x4
    /// </summary>
    Mat4x4 = int.MaxValue - 1,
}

public static class VertexFormatExtensions
{
    public static byte Stride(this VertexFormat format)
    {
        return format switch
        {
            VertexFormat.Uint8x2 => 2,
            VertexFormat.Uint8x4 => 4,
            VertexFormat.Sint8x2 => 2,
            VertexFormat.Sint8x4 => 4,
            VertexFormat.Unorm8x2 => 2,
            VertexFormat.Unorm8x4 => 4,
            VertexFormat.Snorm8x2 => 2,
            VertexFormat.Snorm8x4 => 4,
            VertexFormat.Uint16x2 => 4,
            VertexFormat.Uint16x4 => 8,
            VertexFormat.Sint16x2 => 4,
            VertexFormat.Sint16x4 => 8,
            VertexFormat.Unorm16x2 => 4,
            VertexFormat.Unorm16x4 => 8,
            VertexFormat.Snorm16x2 => 4,
            VertexFormat.Snorm16x4 => 8,
            VertexFormat.Float16x2 => 4,
            VertexFormat.Float16x4 => 8,
            VertexFormat.Float32 => 4,
            VertexFormat.Float32x2 => 8,
            VertexFormat.Float32x3 => 12,
            VertexFormat.Float32x4 => 16,
            VertexFormat.Uint32 => 4,
            VertexFormat.Uint32x2 => 8,
            VertexFormat.Uint32x3 => 12,
            VertexFormat.Uint32x4 => 16,
            VertexFormat.Sint32 => 4,
            VertexFormat.Sint32x2 => 8,
            VertexFormat.Sint32x3 => 12,
            VertexFormat.Sint32x4 => 16,
            VertexFormat.Mat3x3 => 36,
            VertexFormat.Mat4x4 => 64,
            VertexFormat.Mat3x4 => 48,
            _ => throw new InvalidOperationException("Unsupported format"),
        };
    }

    public static int Stride(this VertexFormat[] formats)
    {
        int stride = 0;
        foreach (var format in formats)
        {
            stride += format.Stride();
        }
        return stride;
    }

    public static int Stride(this ReadOnlySpan<VertexFormat> formats)
    {
        int stride = 0;
        foreach (var format in formats)
        {
            stride += format.Stride();
        }
        return stride;
    }

    public static int GetFormatCount(this VertexFormat format)
    {
        return format switch
        {
            VertexFormat.Mat3x3 => 3,
            VertexFormat.Mat4x4 => 4,
            VertexFormat.Mat3x4 => 3,
            _ => 1,
        };
    }

    public static int GetFormatCount(this ReadOnlySpan<VertexFormat> formats)
    {
        int count = 0;
        foreach (var format in formats)
        {
            count += format.GetFormatCount();
        }
        return count;
    }

    public static VertexFormat GetNativeFormat(this VertexFormat format)
    {
        return format switch
        {
            VertexFormat.Mat3x3 => VertexFormat.Float32x3,
            VertexFormat.Mat4x4 => VertexFormat.Float32x4,
            VertexFormat.Mat3x4 => VertexFormat.Float32x4,
            _ => format,
        };
    }
}