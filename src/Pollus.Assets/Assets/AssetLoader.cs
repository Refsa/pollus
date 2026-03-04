namespace Pollus.Assets;

using Pollus.Core.Assets;
using Pollus.Core.Serialization;
using Pollus.Utils;

public enum AssetLoadStatus
{
    Pending,
    Preprocess,
    Loaded,
}

public struct LoadContext
{
    public required AssetPath Path { get; init; }
    public required string FileName { get; init; }
    public required Handle Handle { get; init; }
    public required AssetServer AssetServer { get; init; }
    public required IAssetLoader Loader { get; init; }

    public AssetLoadStatus Status { get; set; }
    public IAsset? Asset { get; private set; }
    public object? State { get; set; }

    public HashSet<Handle>? Dependencies { get; set; }

    public void SetAsset<TAsset>(TAsset asset)
        where TAsset : IAsset
    {
        Asset = asset;
        Status = AssetLoadStatus.Loaded;
    }
}

public interface IAssetLoader
{
    string[] Extensions { get; }
    TypeID AssetType { get; }
    void Load(ReadOnlySpan<byte> data, ref LoadContext context);
    void Resolve(ref LoadContext context) { }
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

        public object? State
        {
            get => context.State;
            set => context.State = value;
        }

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

        public Handle<T> LoadDependency<T>(AssetPath path) where T : IAsset
        {
            var handle = AssetServer.LoadAsync<T>(path);
            AddDependency(handle);
            return handle;
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
        if (context.State is not null && context.Asset is null)
        {
            context.Status = AssetLoadStatus.Preprocess;
        }
    }

    public virtual void Resolve(ref Assets.LoadContext context)
    {
        var wrappedContext = new LoadContext(ref context);
        Resolve(ref wrappedContext);
    }

    protected abstract void Load(ReadOnlySpan<byte> data, ref LoadContext context);
    protected virtual void Resolve(ref LoadContext context) { }
}

public abstract class AssetLoader<TAsset, TState> : AssetLoader<TAsset>
    where TAsset : IAsset
{
    protected sealed override void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        var state = Preprocess(data, ref context);
        context.State = state;
    }

    protected sealed override void Resolve(ref LoadContext context)
    {
        var state = (TState)context.State!;
        Load(state, ref context);
    }

    protected abstract TState Preprocess(ReadOnlySpan<byte> data, ref LoadContext context);
    protected abstract void Load(TState state, ref LoadContext context);
}
