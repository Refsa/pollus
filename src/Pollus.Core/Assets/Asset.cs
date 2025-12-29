namespace Pollus.Core.Assets;

using Utils;

public interface IAsset
{
    HashSet<Handle> Dependencies { get; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class AssetAttribute : Attribute
{
}