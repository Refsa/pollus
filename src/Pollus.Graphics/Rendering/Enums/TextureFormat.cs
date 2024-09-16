namespace Pollus.Graphics.Rendering;

public enum TextureFormat
{
    BC1RgbaUnorm = 43,
    BC1RgbaUnormSrgb = 44,
    BC2RgbaUnorm = 45,
    BC2RgbaUnormSrgb = 46,
    BC3RgbaUnorm = 47,
    BC3RgbaUnormSrgb = 48,
    BC5RGUnorm = 51,
    BC5RGSnorm = 52,
    BC6HrgbUfloat = 53,
    BC6HrgbFloat = 54,
    BC7RgbaUnorm = 55,
    BC7RgbaUnormSrgb = 56,
    Undefined = 0,
    R8Unorm = 1,
    R8Snorm = 2,
    R8Uint = 3,
    R8Sint = 4,
    R16Uint = 5,
    R16Sint = 6,
    R16float = 7,
    RG8Unorm = 8,
    RG8Snorm = 9,
    RG8Uint = 10,
    RG8Sint = 11,
    R32float = 12,
    R32Uint = 13,
    R32Sint = 14,
    RG16Uint = 15,
    RG16Sint = 16,
    RG16float = 17,
    Rgba8Unorm = 18,
    Rgba8UnormSrgb = 19,
    Rgba8Snorm = 20,
    Rgba8Uint = 21,
    Rgba8Sint = 22,
    Bgra8Unorm = 23,
    Bgra8UnormSrgb = 24,
    Rgb10A2Unorm = 25,
    RG11B10Ufloat = 26,
    Rgb9E5Ufloat = 27,
    RG32float = 28,
    RG32Uint = 29,
    RG32Sint = 30,
    Rgba16Uint = 31,
    Rgba16Sint = 32,
    Rgba16float = 33,
    Rgba32float = 34,
    Rgba32Uint = 35,
    Rgba32Sint = 36,
    Stencil8 = 37,
    Depth16Unorm = 38,
    Depth24Plus = 39,
    Depth24PlusStencil8 = 40,
    Depth32float = 41,
    Depth32floatStencil8 = 42,
    BC1Rgbaunorm = 43,
    BC1RgbaunormSrgb = 44,
    BC2Rgbaunorm = 45,
    BC2RgbaunormSrgb = 46,
    BC3Rgbaunorm = 47,
    BC3RgbaunormSrgb = 48,
    BC4RUnorm = 49,
    BC4RSnorm = 50,
    BC5Rgunorm = 51,
    BC5Rgsnorm = 52,
    BC6Hrgbufloat = 53,
    BC6Hrgbfloat = 54,
    BC7Rgbaunorm = 55,
    BC7RgbaunormSrgb = 56,
    Etc2Rgb8Unorm = 57,
    Etc2Rgb8UnormSrgb = 58,
    Etc2Rgb8A1Unorm = 59,
    Etc2Rgb8A1UnormSrgb = 60,
    Etc2Rgba8Unorm = 61,
    Etc2Rgba8UnormSrgb = 62,
    Eacr11Unorm = 63,
    Eacr11Snorm = 64,
    Eacrg11Unorm = 65,
    Eacrg11Snorm = 66,
    Astc4x4Unorm = 67,
    Astc4x4UnormSrgb = 68,
    Astc5x4Unorm = 69,
    Astc5x4UnormSrgb = 70,
    Astc5x5Unorm = 71,
    Astc5x5UnormSrgb = 72,
    Astc6x5Unorm = 73,
    Astc6x5UnormSrgb = 74,
    Astc6x6Unorm = 75,
    Astc6x6UnormSrgb = 76,
    Astc8x5Unorm = 77,
    Astc8x5UnormSrgb = 78,
    Astc8x6Unorm = 79,
    Astc8x6UnormSrgb = 80,
    Astc8x8Unorm = 81,
    Astc8x8UnormSrgb = 82,
    Astc10x5Unorm = 83,
    Astc10x5UnormSrgb = 84,
    Astc10x6Unorm = 85,
    Astc10x6UnormSrgb = 86,
    Astc10x8Unorm = 87,
    Astc10x8UnormSrgb = 88,
    Astc10x10Unorm = 89,
    Astc10x10UnormSrgb = 90,
    Astc12x10Unorm = 91,
    Astc12x10UnormSrgb = 92,
    Astc12x12Unorm = 93,
    Astc12x12UnormSrgb = 94,
}

public static class TextureFormatExt
{
    public static uint BytesPerPixel(this TextureFormat descriptor)
    {
        return descriptor switch
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
            _ => throw new IndexOutOfRangeException($"Unknown texture format: {descriptor}")
        };
    }
}