namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

public struct WGPUStorageTextureBindingLayout
{
    public static readonly StorageTextureBindingLayout Undefined = new()
    {
        Access = StorageTextureAccess.Undefined,
        Format = TextureFormat.Undefined,
        ViewDimension = TextureViewDimension.DimensionUndefined,
    };

    public StorageTextureAccess Access;
    public TextureFormat Format;
    public TextureViewDimension ViewDimension;
}