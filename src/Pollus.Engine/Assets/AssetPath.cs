namespace Pollus.Engine.Assets;

public record struct AssetPath(string Path)
{
    public static implicit operator AssetPath(string path) => new(path);
}