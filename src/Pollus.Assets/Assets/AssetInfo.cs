namespace Pollus.Assets;

using Pollus.Utils;
using Pollus.Core.Assets;

public enum AssetStatus
{
    Failed = -1,
    Unknown = 0,
    Initialized,
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

    public void SetDependencies(HashSet<Handle>? dependencies)
    {
        Dependencies = dependencies;
    }

    public void AddDependent(Handle dependent)
    {
        Dependents.Add(dependent);
    }
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
