namespace Pollus.Engine.Assets;

using System.Collections.Concurrent;
using Debugging;
using ECS;

public class DevelopmentAssetsWatch : IDisposable
{
    class CopyTask
    {
        public required string SourcePath { get; init; }
        public required string DestinationPath { get; init; }
    }

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From(() => AssetPlugin.Default),
    ];

    const int DEBOUNCE_TIME = 300;

    FileSystemWatcher watcher;
    string sourcePath;

    ConcurrentDictionary<string, Timer> debounceTimers = new();

    public static DevelopmentAssetsWatch? Create()
    {
        var sourcePath = GetAssetPath();
        if (sourcePath is null) return null;
        return new DevelopmentAssetsWatch(sourcePath);
    }

    public DevelopmentAssetsWatch(string? devAssetsPath)
    {
        sourcePath = devAssetsPath ?? GetAssetPath() ?? throw new InvalidOperationException("PollusAssetsPath needs to be set in project file");
        watcher = new FileSystemWatcher(sourcePath);
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        watcher.Changed += OnFileChanged;
    }

    public void Dispose()
    {
        watcher?.Dispose();
    }

    void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (debounceTimers.TryGetValue(e.FullPath, out var task))
        {
            task.Change(DEBOUNCE_TIME, Timeout.Infinite);
            return;
        }

        var relative = Path.GetRelativePath(sourcePath, e.FullPath);
        var binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
        var dest = Path.Combine(binPath, relative);

        debounceTimers.TryAdd(e.FullPath, new Timer(CopyCallback, new CopyTask()
        {
            SourcePath = e.FullPath,
            DestinationPath = dest
        }, DEBOUNCE_TIME, Timeout.Infinite));
    }

    void CopyCallback(object? state)
    {
        if (state is not CopyTask copyTask) return;
        File.Copy(copyTask.SourcePath, copyTask.DestinationPath, true);
        debounceTimers.TryRemove(copyTask.SourcePath, out _);
    }

    static string? GetAssetPath()
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        var attribute = assembly?.GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), true)
            .Cast<System.Reflection.AssemblyMetadataAttribute>()
            .FirstOrDefault(x => x.Key == "PollusAssetsPath");

        return attribute?.Value;
    }
}