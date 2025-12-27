namespace Pollus.Core.Assets;

using System.Collections.ObjectModel;
using Utils;

public interface IAsset
{
    ReadOnlySet<Handle> Dependencies { get; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class AssetAttribute : Attribute
{
}