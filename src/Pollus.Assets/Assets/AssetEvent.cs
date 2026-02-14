namespace Pollus.Assets;

using Pollus.Utils;

public enum AssetEventType
{
    Added,
    Loaded,
    Unloaded,
    Changed,
    Deleted,
    Failed,
}

public struct AssetEvent<TAsset>
    where TAsset : notnull
{
    public required AssetEventType Type { get; init; }
    public required Handle<TAsset> Handle { get; init; }
}
