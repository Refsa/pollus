namespace Pollus.Engine.Rendering;

public enum ImageAssetFormat
{
    RGBA8,
    RGB8,
    RG8,
    R8,
}

public class ImageAsset
{
    public required string Name { get; set; }
    public required byte[] Data { get; init; }
    public required ImageAssetFormat Format { get; init; }
    public required uint Width { get; init; }
    public uint Height { get; init; }
    public uint Depth { get; init; }
}
