namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

public struct WGPUTextureBindingLayout
{
    public static readonly TextureBindingLayout Undefined = new()
    {
        SampleType = TextureSampleType.Undefined,
        ViewDimension = TextureViewDimension.DimensionUndefined,
        Multisampled = false,
    };

    public TextureSampleType SampleType;
    public TextureViewDimension ViewDimension;
    public bool Multisampled;
}
