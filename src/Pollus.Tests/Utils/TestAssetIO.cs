namespace Pollus.Tests.Utils;

using Pollus.Engine.Assets;

public class TestAssetIO : AssetIO
{
    Dictionary<AssetPath, byte[]> content = new();

    public TestAssetIO(string rootPath) : base(rootPath)
    {
    }

    public TestAssetIO(params (AssetPath, byte[])[] files) : base("assets")
    {
        foreach (var (path, data) in files)
        {
            content.Add(path, data);
        }
    }

    public TestAssetIO AddFile(AssetPath path, byte[] data)
    {
        content.Add(path, data);
        return this;
    }

    public override bool Exists(in AssetPath path)
    {
        return content.ContainsKey(path);
    }

    public override bool IsDirectory(in AssetPath path)
    {
        return content.ContainsKey(path);
    }

    public override bool IsFile(in AssetPath path)
    {
        return content.ContainsKey(path);
    }

    public override Result GetDirectoryContent(in AssetPath path, out List<AssetPath> content)
    {
        content = [];
        return Result.Success;
    }

    public override Result LoadPath(in AssetPath path, out byte[] content)
    {
        if (this.content.TryGetValue(path, out var data))
        {
            content = data;
            return Result.Success;
        }

        content = Array.Empty<byte>();
        return Result.FileNotFound;
    }

    public override Task<byte[]> LoadPathAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}