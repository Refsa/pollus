namespace Pollus.Engine.Assets;

using System.Text;

public class TextAsset
{
    public string Content { get; }

    public TextAsset(string content)
    {
        Content = content;
    }
}

public class TextAssetLoader : AssetLoader<TextAsset>
{
    public override string[] Extensions => [".txt"];

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext<TextAsset> context)
    {
        var asset = new TextAsset(Encoding.UTF8.GetString(data));
        context.SetAsset(asset);
    }
}