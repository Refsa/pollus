namespace Pollus.Engine.Assets;

using Pollus.Engine.Serialization;
using Pollus.Core.Serialization;
using Pollus.Utils;

public struct LoadContext
{
    public required AssetPath Path { get; init; }
    public required string FileName { get; init; }
    public required Handle Handle { get; init; }
    public required AssetServer AssetServer { get; init; }

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

public abstract class AssetLoader<TAsset> : IAssetLoader
    where TAsset : notnull
{
    protected ref struct LoadContext<T>
    {
        ref LoadContext context;
        public readonly AssetPath Path => context.Path;
        public readonly string FileName => context.FileName;
        public readonly Handle Handle => context.Handle;
        public readonly Assets Assets => context.AssetServer.Assets;

        public LoadContext(ref LoadContext context)
        {
            this.context = ref context;
        }
        public void SetAsset(T asset) => context.SetAsset(asset);
    }

    static AssetLoader()
    {
        AssetsFetch<TAsset>.Register();
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<TAsset>());
    }

    static readonly int _assetType = TypeLookup.ID<TAsset>();
    public int AssetType => _assetType;

    public abstract string[] Extensions { get; }

    public void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        var wrappedContext = new LoadContext<TAsset>(ref context);
        Load(data, ref wrappedContext);
    }

    protected abstract void Load(ReadOnlySpan<byte> data, ref LoadContext<TAsset> context);
}