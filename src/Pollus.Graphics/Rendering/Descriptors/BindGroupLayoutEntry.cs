namespace Pollus.Graphics.Rendering;

using Pollus.Utils;

public struct BindGroupLayoutEntry
{
    public static readonly BindGroupLayoutEntry Undefined = new()
    {
        Binding = 0,
        Visibility = ShaderStage.None,
        Buffer = BufferBindingLayout.Undefined,
        Sampler = SamplerBindingLayout.Undefined,
        Texture = TextureBindingLayout.Undefined,
        StorageTexture = StorageTextureBindingLayout.Undefined,
    };

    public uint Binding;
    public ShaderStage Visibility;

    public BufferBindingLayout Buffer;
    public SamplerBindingLayout Sampler;
    public TextureBindingLayout Texture;
    public StorageTextureBindingLayout StorageTexture;

    public static BindGroupLayoutEntry BufferEntry<T>(uint binding, ShaderStage visibility, BufferBindingType bindingType, bool hasDynamicOffset = false)
        where T : unmanaged
    {
        return Undefined with
        {
            Binding = binding,
            Visibility = visibility,
            Buffer = BufferBindingLayout.Undefined with
            {
                Type = bindingType,
                MinBindingSize = Alignment.GPUAlignedSize<T>(1),
                HasDynamicOffset = hasDynamicOffset
            },
        };
    }

    public static BindGroupLayoutEntry Uniform<T>(uint binding, ShaderStage visibility, bool hasDynamicOffset = false)
        where T : unmanaged
    {
        return BufferEntry<T>(binding, visibility, BufferBindingType.Uniform, hasDynamicOffset);
    }

    public static BindGroupLayoutEntry SamplerEntry(uint binding, ShaderStage visibility, SamplerBindingType type)
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

    public static BindGroupLayoutEntry TextureEntry(uint binding, ShaderStage visibility,
        TextureSampleType sampleType, TextureViewDimension viewDimension, bool multisampled = false)
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

    public static BindGroupLayoutEntry StorageTextureEntry(uint binding, ShaderStage visibility,
        StorageTextureAccess access, TextureFormat format, TextureViewDimension viewDimension)
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
