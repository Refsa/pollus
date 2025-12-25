namespace Pollus.Engine.Assets;

using Utils;

public abstract class AssetIO : IDisposable
{
    public enum ErrorType
    {
        EmptyFile = 0,
        PathNotFound,
        FileNotFound,
        DirectoryNotFound,
        Exception,
    }

    public readonly struct Error : IError
    {
        public static string Name => "AssetIOError";

        public required ErrorType Result { get; init; }
        public string Inner { get; init; }

        public static implicit operator Error(ErrorType result)
        {
            return From(result);
        }

        public static Error From(ErrorType result)
        {
            return new Error()
            {
                Result = result,
                Inner = result.ToString(),
            };
        }

        public static Error Exception(Exception exception)
        {
            return new Error()
            {
                Result = ErrorType.Exception,
                Inner = exception.Message,
            };
        }
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
    public abstract Result<List<AssetPath>, Error> GetDirectoryContent(in AssetPath path);
    public abstract Result<byte[], Error> LoadPath(in AssetPath path);
    public abstract Task<Result<byte[], Error>> LoadPathAsync(AssetPath path, CancellationToken cancellationToken = default);

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