namespace Pollus.Engine.Assets;

using Utils;

public enum AssetEventType
{
    Added,
    Unloaded,
    Changed,
    DependenciesChanged,
    Deleted,
}

public struct AssetEvent<TAsset>
    where TAsset : notnull
{
    public required AssetEventType Type { get; init; }
    public required Handle<TAsset> Handle { get; init; }
}