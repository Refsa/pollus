namespace Pollus.Graphics.WGPU;

using Pollus.Utils;
using Silk.NET.WebGPU;

public struct WGPUBindGroupLayoutEntry
{
    public static readonly BindGroupLayoutEntry Undefined = new()
    {
        Binding = 0,
        Visibility = ShaderStage.None,
        Buffer = WGPUBufferBindingLayout.Undefined,
        Sampler = WGPUSamplerBindingLayout.Undefined,
        Texture = WGPUTextureBindingLayout.Undefined,
        StorageTexture = WGPUStorageTextureBindingLayout.Undefined,
    };

    public uint Binding;
    public ShaderStage Visibility;

    public WGPUBufferBindingLayout Buffer;
    public WGPUSamplerBindingLayout Sampler;
    public WGPUTextureBindingLayout Texture;
    public WGPUStorageTextureBindingLayout StorageTexture;

    public static BindGroupLayoutEntry BufferEntry<T>(uint binding, ShaderStage visibility, BufferBindingType bindingType, bool hasDynamicOffset = false)
        where T : unmanaged
    {
        return Undefined with
        {
            Binding = binding,
            Visibility = visibility,
            Buffer = WGPUBufferBindingLayout.Undefined with
            {
                Type = bindingType,
                MinBindingSize = Alignment.GetAlignedSize<T>(true),
                HasDynamicOffset = hasDynamicOffset
            },
        };
    }

    public static BindGroupLayoutEntry SamplerEntry(uint binding, ShaderStage visibility, SamplerBindingType type)
    {
        return Undefined with
        {
            Binding = binding,
            Visibility = visibility,
            Sampler = WGPUSamplerBindingLayout.Undefined with
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
            Texture = WGPUTextureBindingLayout.Undefined with
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
            StorageTexture = WGPUStorageTextureBindingLayout.Undefined with
            {
                Access = access,
                Format = format,
                ViewDimension = viewDimension,
            }
        };
    }
}
