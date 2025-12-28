namespace Pollus.Engine.Assets;

using Pollus.Utils;
using Core.Assets;

public enum AssetStatus
{
    Failed = -1,
    Unknown = 0,
    Unloaded = 1,
    Initialized,
    Loading,
    Added,
    WaitingForDependency,
    Loaded,
}

public interface IAssetInfo
{
    public Handle Handle { get; set; }
    public AssetStatus Status { get; set; }
    public AssetPath? Path { get; set; }

    public HashSet<Handle>? Dependencies { get; set; }
    public HashSet<Handle> Dependents { get; set; }
    public DateTime LastModified { get; set; }
}

public class AssetInfo<T> : IAssetInfo
    where T : IAsset
{
    public T? Asset { get; set; }

    public Handle Handle { get; set; }
    public AssetStatus Status { get; set; }
    public AssetPath? Path { get; set; }

    public HashSet<Handle>? Dependencies { get; set; }
    public HashSet<Handle> Dependents { get; set; } = [];
    public DateTime LastModified { get; set; }
}