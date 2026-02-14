namespace Pollus.Assets;

using Pollus.Core.Assets;
using Pollus.Core.Serialization;
using Pollus.Utils;

public struct LoadContext
{
    public required AssetPath Path { get; init; }
    public required string FileName { get; init; }
    public required Handle Handle { get; init; }
    public required AssetServer AssetServer { get; init; }

    public AssetStatus Status { get; set; }
    public IAsset? Asset { get; private set; }

    public HashSet<Handle>? Dependencies { get; set; }

    public void SetAsset<TAsset>(TAsset asset)
        where TAsset : IAsset
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
    where TAsset : IAsset
{
    protected ref struct LoadContext
    {
        ref Assets.LoadContext context;
        public readonly AssetPath Path => context.Path;
        public readonly string FileName => context.FileName;
        public readonly Handle Handle => context.Handle;
        public readonly AssetServer AssetServer => context.AssetServer;

        public LoadContext(ref Assets.LoadContext context)
        {
            this.context = ref context;
        }

        public void SetAsset(TAsset asset) => context.SetAsset(asset);

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

    public void Load(ReadOnlySpan<byte> data, ref Assets.LoadContext context)
    {
        var wrappedContext = new LoadContext(ref context);
        Load(data, ref wrappedContext);
    }

    protected abstract void Load(ReadOnlySpan<byte> data, ref LoadContext context);
}
