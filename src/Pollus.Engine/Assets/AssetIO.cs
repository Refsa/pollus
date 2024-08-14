namespace Pollus.Engine.Assets;

public record struct AssetPath(string Path)
{
    public static implicit operator AssetPath(string path) => new(path);
}

public abstract class AssetIO
{
    public enum Result
    {
        EmptyFile = -4,
        PathNotFound = -3,
        FileNotFound = -2,
        DirectoryNotFound = -1,
        Success = 1,
    }

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
}

public class FileAssetIO : AssetIO
{
    public FileAssetIO(string rootPath) : base(rootPath)
    {
    }

    public override bool Exists(in AssetPath path)
    {
        return File.Exists(BuildPath(path));
    }

    public override bool IsDirectory(in AssetPath path)
    {
        return Directory.Exists(BuildPath(path));
    }

    public override bool IsFile(in AssetPath path)
    {
        return File.Exists(BuildPath(path));
    }

    public override Result GetDirectoryContent(in AssetPath path, out List<AssetPath> content)
    {
        content = [];
        if (IsDirectory(path) is false)
        {
            return Result.DirectoryNotFound;
        }

        RecursiveScan(BuildPath(path), content);
        return Result.Success;

        static void RecursiveScan(string folder, List<AssetPath> content)
        {
            foreach (var file in Directory.GetFiles(folder))
            {
                var assetPath = new AssetPath(file.Replace(Directory.GetCurrentDirectory(), ""));
                content.Add(assetPath);
            }

            foreach (var dir in Directory.GetDirectories(folder))
            {
                RecursiveScan(dir, content);
            }
        }
    }

    public override Result LoadPath(in AssetPath path, out byte[] content)
    {
        if (Exists(path) is false)
        {
            content = Array.Empty<byte>();
            return Result.FileNotFound;
        }

        content = File.ReadAllBytes(BuildPath(path));
        return Result.Success;
    }
}