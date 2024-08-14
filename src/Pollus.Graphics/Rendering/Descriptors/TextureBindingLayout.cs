namespace Pollus.Graphics.Rendering;

public struct TextureBindingLayout
{
    public static readonly TextureBindingLayout Undefined = new()
    {
        SampleType = Silk.NET.WebGPU.TextureSampleType.Undefined,
        ViewDimension = Silk.NET.WebGPU.TextureViewDimension.DimensionUndefined,
        Multisampled = false,
    };

    public Silk.NET.WebGPU.TextureSampleType SampleType;
    public Silk.NET.WebGPU.TextureViewDimension ViewDimension;
    public bool Multisampled;
}
