namespace Pollus.Graphics.Rendering;

using Pollus.Utils;

public struct BindGroupLayoutEntry
{
    public static readonly BindGroupLayoutEntry Undefined = new()
    {
        Binding = 0,
        Visibility = Silk.NET.WebGPU.ShaderStage.None,
        Buffer = BufferBindingLayout.Undefined,
        Sampler = SamplerBindingLayout.Undefined,
        Texture = TextureBindingLayout.Undefined,
        StorageTexture = StorageTextureBindingLayout.Undefined,
    };

    public uint Binding;
    public Silk.NET.WebGPU.ShaderStage Visibility;

    public BufferBindingLayout Buffer;
    public SamplerBindingLayout Sampler;
    public TextureBindingLayout Texture;
    public StorageTextureBindingLayout StorageTexture;

    public static BindGroupLayoutEntry BufferEntry<T>(uint binding, Silk.NET.WebGPU.ShaderStage visibility, Silk.NET.WebGPU.BufferBindingType bindingType, bool hasDynamicOffset = false)
        where T : unmanaged
    {
        return Undefined with
        {
            Binding = binding,
            Visibility = visibility,
            Buffer = BufferBindingLayout.Undefined with
            {
                Type = bindingType,
                MinBindingSize = Alignment.GetAlignedSize<T>(true),
                HasDynamicOffset = hasDynamicOffset
            },
        };
    }

    public static BindGroupLayoutEntry Uniform<T>(uint binding, Silk.NET.WebGPU.ShaderStage visibility, bool hasDynamicOffset = false)
        where T : unmanaged
    {
        return BufferEntry<T>(binding, visibility, Silk.NET.WebGPU.BufferBindingType.Uniform, hasDynamicOffset);
    }

    public static BindGroupLayoutEntry SamplerEntry(uint binding, Silk.NET.WebGPU.ShaderStage visibility, Silk.NET.WebGPU.SamplerBindingType type)
    {
        return Undefined with
        {
            Binding = binding,
            Visibility = visibility,
            Sampler = SamplerBindingLayout.Undefined with
            {
                Type = type,
            },
        };
    }

    public static BindGroupLayoutEntry TextureEntry(uint binding, Silk.NET.WebGPU.ShaderStage visibility,
        Silk.NET.WebGPU.TextureSampleType sampleType, Silk.NET.WebGPU.TextureViewDimension viewDimension, bool multisampled = false)
    {
        return Undefined with
        {
            Binding = binding,
            Visibility = visibility,
            Texture = TextureBindingLayout.Undefined with
            {
                SampleType = sampleType,
                ViewDimension = viewDimension,
                Multisampled = multisampled,
            }
        };
    }

    public static BindGroupLayoutEntry StorageTextureEntry(uint binding, Silk.NET.WebGPU.ShaderStage visibility,
        Silk.NET.WebGPU.StorageTextureAccess access, Silk.NET.WebGPU.TextureFormat format, Silk.NET.WebGPU.TextureViewDimension viewDimension)
    {
        return Undefined with
        {
            Binding = binding,
            Visibility = visibility,
            StorageTexture = StorageTextureBindingLayout.Undefined with
            {
                Access = access,
                Format = format,
                ViewDimension = viewDimension,
            }
        };
    }
}
