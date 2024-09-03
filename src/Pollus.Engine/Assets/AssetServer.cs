namespace Pollus.Engine.Assets;

using Pollus.Utils;

public class AssetServer : IDisposable
{
    public AssetIO AssetIO { get; }
    public Assets Assets { get; } = new();

    List<IAssetLoader> loaders = new();
    Dictionary<string, int> loaderLookup = new();
    Dictionary<AssetPath, Handle> assetLookup = new();

    public AssetServer(AssetIO assetIO)
    {
        AssetIO = assetIO;
    }

    public void Dispose()
    {
        Assets.Dispose();
    }

    public AssetServer AddLoader<TLoader>() where TLoader : IAssetLoader, new()
    {
        var idx = loaders.Count;
        TLoader loader = new();
        loaders.Add(loader);
        foreach (var ext in loader.Extensions)
        {
            loaderLookup.Add(ext, idx);
        }
        return this;
    }

    public AssetServer AddLoader<TLoader>(TLoader loader) where TLoader : IAssetLoader
    {
        var idx = loaders.Count;
        loaders.Add(loader);
        foreach (var ext in loader.Extensions)
        {
            loaderLookup.Add(ext, idx);
        }
        return this;
    }

    public Assets<TAsset> GetAssets<TAsset>()
        where TAsset : notnull
    {
        return Assets.GetAssets<TAsset>();
    }

    public Handle<TAsset> Load<TAsset>(AssetPath path)
        where TAsset : notnull
    {
        if (assetLookup.TryGetValue(path, out var handle))
        {
            return handle;
        }

        if (!AssetIO.Exists(path))
        {
            return new Handle(-1, -1);
        }

        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx))
        {
            return new Handle(-1, -1);
        }

        var loader = loaders[loaderIdx];
        var loadContext = new LoadContext()
        {
            Path = path,
            FileName = Path.GetFileNameWithoutExtension(path.Path)
        };

        AssetIO.LoadPath(path, out var data);
        loader.Load(data, ref loadContext);
        if (loadContext.Status == AssetStatus.Loaded)
        {
            var asset = (TAsset)loadContext.Asset!;
            handle = Assets.Add(asset);
            assetLookup.Add(path, handle);
            return handle;
        }

        return new Handle(-1, -1);
    }
}