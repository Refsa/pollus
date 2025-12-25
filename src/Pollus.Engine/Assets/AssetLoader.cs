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

    public List<Handle>? Dependencies { get; set; }

    public void SetAsset<T>(T asset)
    {
        Asset = asset;
        Status = AssetStatus.Loaded;
    }
}

public interface IAssetLoader
{
    string[] Extensions { get; }
    TypeID AssetType { get; }
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
        public readonly AssetsContainer Assets => context.AssetServer.Assets;
        public readonly AssetServer AssetServer => context.AssetServer;

        public LoadContext(ref LoadContext context)
        {
            this.context = ref context;
        }

        public void SetAsset(T asset) => context.SetAsset(asset);

        public void AddDependency(Handle dependency)
        {
            context.Dependencies ??= [];
            context.Dependencies.Add(dependency);
        }
    }

    static AssetLoader()
    {
        AssetsFetch<TAsset>.Register();
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<TAsset>());
    }

    static readonly TypeID _assetType = TypeLookup.ID<TAsset>();
    public TypeID AssetType => _assetType;

    public abstract string[] Extensions { get; }

    public void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        var wrappedContext = new LoadContext<TAsset>(ref context);
        Load(data, ref wrappedContext);
    }

    protected abstract void Load(ReadOnlySpan<byte> data, ref LoadContext<TAsset> context);
}