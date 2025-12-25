namespace Pollus.Engine.Assets;

using Utils;

public class FileAssetIO : AssetIO
{
    FileSystemWatcher? watcher;

    public FileAssetIO(string rootPath) : base(rootPath)
    {
    }

    public override void Dispose()
    {
        watcher?.Dispose();
    }

    public override void Watch()
    {
        watcher = new FileSystemWatcher(Path.GetFullPath(RootPath));
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        watcher.Changed += OnFileChanged;
    }

    void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var relativePath = Path.GetRelativePath(Path.GetFullPath(RootPath), e.FullPath);
        relativePath = relativePath.Replace('\\', '/');
        NotifyAssetChanged(new AssetPath(relativePath));
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

    public override Result<List<AssetPath>, Error> GetDirectoryContent(in AssetPath path)
    {
        if (IsDirectory(path) is false)
        {
            return Error.From(ErrorType.DirectoryNotFound);
        }

        var content = new List<AssetPath>();
        RecursiveScan(BuildPath(path), content);
        return content;

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

    public override Result<byte[], Error> LoadPath(in AssetPath path)
    {
        if (Exists(path) is false)
        {
            return Error.From(ErrorType.FileNotFound);
        }

        return File.ReadAllBytes(BuildPath(path));
    }

    public override async Task<Result<byte[], Error>> LoadPathAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        try
        {
            return await File.ReadAllBytesAsync(BuildPath(path), cancellationToken);
        }
        catch (Exception e)
        {
            return Error.Exception(e);
        }
    }
}