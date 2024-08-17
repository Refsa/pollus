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
    public required int Width { get; init; }
    public int Height { get; init; }
    public int Depth { get; init; }
}
