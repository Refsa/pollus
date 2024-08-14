namespace Pollus.Engine.Assets;

public class AssetServer
{
    public AssetIO AssetIO { get; }
    public Assets Assets { get; } = new();

    List<IAssetLoader> loaders = new();
    Dictionary<string, int> loaderLookup = new();

    public AssetServer(AssetIO assetIO)
    {
        AssetIO = assetIO;
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

    public Handle Load<TAsset>(AssetPath path) 
        where TAsset : notnull
    {
        if (!AssetIO.Exists(path))
        {
            return new Handle(-1, -1);
        }

        if (!loaderLookup.TryGetValue(Path.GetExtension(path.Path), out var loaderIdx))
        {
            return new Handle(-1, -1);
        }

        var loader = loaders[loaderIdx];
        var loadContext = new LoadContext();
        AssetIO.LoadPath(path, out var data);
        loader.Load(data, ref loadContext);
        if (loadContext.Status == AssetStatus.Loaded)
        {
            var asset = (TAsset)loadContext.Asset!;
            return Assets.Add(asset);
        }

        return new Handle(-1, -1);
    }
}