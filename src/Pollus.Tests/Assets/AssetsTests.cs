namespace Pollus.Tests.Assets;

using Pollus.Engine.Assets;

public class AssetsTests
{
    [Fact]
    public void TextAssetLoader()
    {
        var assetLoader = new TextAssetLoader();
        var loadContext = new LoadContext()
        {
            Path = new AssetPath("test.txt"),
            FileName = "test.txt",
        };

        assetLoader.Load(File.ReadAllBytes("Assets/TestFiles/test.txt"), ref loadContext);
        Assert.Equal(AssetStatus.Loaded, loadContext.Status);
        Assert.Equal("this is some text", ((TextAsset)loadContext.Asset!).Content);
    }

    [Fact]
    public void AssetLoader_LoadAsset()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle = assetServer.Load<TextAsset>(new AssetPath("test.txt"));
        var asset = assetServer.Assets.Get<TextAsset>(handle);

        Assert.NotNull(asset);
        Assert.Equal("this is some text", asset.Content);
    }
}