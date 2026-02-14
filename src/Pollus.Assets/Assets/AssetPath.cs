namespace Pollus.Assets;

using System.Diagnostics;

[DebuggerDisplay("{Path}")]
public readonly record struct AssetPath(string Path)
{
    public static implicit operator AssetPath(string path) => new(path);

    public override string ToString() => $"AssetPath(\"{Path}\")";
}
