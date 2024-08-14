namespace Pollus.Graphics.Rendering;

public struct StorageTextureBindingLayout
{
    public static readonly StorageTextureBindingLayout Undefined = new()
    {
        Access = Silk.NET.WebGPU.StorageTextureAccess.Undefined,
        Format = Silk.NET.WebGPU.TextureFormat.Undefined,
        ViewDimension = Silk.NET.WebGPU.TextureViewDimension.DimensionUndefined,
    };

    public Silk.NET.WebGPU.StorageTextureAccess Access;
    public Silk.NET.WebGPU.TextureFormat Format;
    public Silk.NET.WebGPU.TextureViewDimension ViewDimension;
}