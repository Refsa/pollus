namespace Pollus.Tests.Utils;

using Pollus.Engine.Assets;
using Pollus.Utils;

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

    public override Result<List<AssetPath>, Error> GetDirectoryContent(in AssetPath path)
    {
        return content.Keys.ToList();
    }

    public override Result<byte[], Error> LoadPath(in AssetPath path)
    {
        return content[path];
    }

    public override Task<Result<byte[], Error>> LoadPathAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}