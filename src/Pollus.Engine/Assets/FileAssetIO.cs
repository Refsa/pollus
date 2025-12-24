namespace Pollus.Engine.Assets;

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

    public override async Task<byte[]> LoadPathAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllBytesAsync(BuildPath(path), cancellationToken);
    }
}