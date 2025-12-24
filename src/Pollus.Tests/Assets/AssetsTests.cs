namespace Pollus.Tests.Assets;

using System.Diagnostics;
using Pollus.Engine.Assets;
using Pollus.Utils;

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
            Handle = new Handle<TextAsset>(1),
            AssetServer = null!,
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
        var asset = assetServer.Assets.Get(handle);

        Assert.NotNull(asset);
        Assert.Equal("this is some text", asset.Content);
    }

    [Fact]
    public async Task AssetLoader_LoadAssetAsync()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle = assetServer.LoadAsync<TextAsset>(new AssetPath("test.txt"));

        var stopwatch = Stopwatch.StartNew();
        while (assetServer.Assets.GetStatus<TextAsset>(handle) != AssetStatus.Loaded)
        {
            assetServer.Update();
            await Task.Delay(10);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000);
        }

        stopwatch.Stop();

        var asset = assetServer.Assets.Get(handle);

        Assert.NotNull(asset);
        Assert.Equal("this is some text", asset.Content);
    }
}