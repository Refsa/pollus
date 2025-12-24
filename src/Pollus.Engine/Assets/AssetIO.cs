namespace Pollus.Engine.Assets;

public abstract class AssetIO : IDisposable
{
    public enum Result
    {
        EmptyFile = -4,
        PathNotFound = -3,
        FileNotFound = -2,
        DirectoryNotFound = -1,
        Success = 1,
    }

    public event Action<AssetPath>? OnAssetChanged;

    public string RootPath { get; }

    public AssetIO(string rootPath)
    {
        RootPath = rootPath;
    }

    public string BuildPath(in AssetPath path)
    {
        return Path.Combine(RootPath, path.Path switch
        {
            var p when p.StartsWith('/') || p.StartsWith('\\') => p[1..],
            var p => p,
        });
    }

    public abstract bool Exists(in AssetPath path);
    public abstract bool IsDirectory(in AssetPath path);
    public abstract bool IsFile(in AssetPath path);
    public abstract Result GetDirectoryContent(in AssetPath path, out List<AssetPath> content);
    public abstract Result LoadPath(in AssetPath path, out byte[] content);

    protected void NotifyAssetChanged(AssetPath path)
    {
        OnAssetChanged?.Invoke(path);
    }

    public virtual void Watch()
    {
        throw new NotSupportedException("Watching is not supported by this asset IO");
    }

    public virtual void Dispose()
    {
        
    }
}