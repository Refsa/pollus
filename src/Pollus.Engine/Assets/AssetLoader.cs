namespace Pollus.Engine.Assets;

public struct LoadContext
{
    public AssetStatus Status { get; set; }
    public object? Asset { get; private set; }

    public void SetAsset<T>(T asset)
    {
        Asset = asset;
        Status = AssetStatus.Loaded;
    }
}

public interface IAssetLoader
{
    string[] Extensions { get; }
    int AssetType { get; }
    void Load(ReadOnlySpan<byte> data, ref LoadContext context);
}

public abstract class AssetLoader<T> : IAssetLoader
    where T : notnull
{
    static AssetLoader()
    {
        AssetsFetch<T>.Register();
    }

    static readonly int _assetType = AssetLookup.ID<T>();
    public int AssetType => _assetType;

    public abstract string[] Extensions { get; }
    public abstract void Load(ReadOnlySpan<byte> data, ref LoadContext context);
}