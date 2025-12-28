namespace Pollus.Tests.Assets;

using System.Diagnostics;
using Core.Assets;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Utils;

[Asset]
partial class TestAsset
{
    public float Value { get; set; }
    public Handle<TextAsset> Dependency { get; set; }
}

[Asset]
partial class ParentTestAsset
{
    public float Value { get; set; }
    public required ContainsAsset Child { get; set; }
}

[Asset]
partial class ContainsAsset
{
    public Handle<TextAsset> Dependency { get; set; }
}

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
        var asset = assetServer.Assets.GetAsset(handle);

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

        var asset = assetServer.Assets.GetAsset(handle);

        Assert.NotNull(asset);
        Assert.Equal("this is some text", asset.Content);
    }

    [Fact]
    public void AssetsContainer_SetDependencies()
    {
        var assetsContainer = new AssetsContainer();
        var assets = assetsContainer.InitAssets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));
        assetsContainer.SetDependencies(handle1, [handle2]);

        var asset1Info = assets.GetInfo(handle1);
        Assert.NotNull(asset1Info);
        Assert.NotNull(asset1Info.Dependencies);
        Assert.Single(asset1Info.Dependencies);
        Assert.Equal(handle2, asset1Info.Dependencies.First().As<TextAsset>());

        var asset2Info = assets.GetInfo(handle2);
        Assert.NotNull(asset2Info);
        Assert.NotNull(asset2Info.Dependencies);
        Assert.Empty(asset2Info.Dependencies);
        Assert.Single(asset2Info.Dependents);
        Assert.Equal(handle1, asset2Info.Dependents.First().As<TextAsset>());
    }

    [Fact]
    public void AssetsContainer_NotifyDependants_SingleDependency()
    {
        var assetsContainer = new AssetsContainer();
        var assets = assetsContainer.InitAssets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));
        assetsContainer.SetDependencies(handle1, [handle2]);
        assetsContainer.ClearEvents();

        var events = new Events();
        assetsContainer.NotifyDependants(handle2);
        assetsContainer.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(1, assetEvents.Length);
    }

    [Fact]
    public void AssetsContainer_NotifyDependants_DeepDependency()
    {
        var assetsContainer = new AssetsContainer();
        var assets = assetsContainer.InitAssets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));
        var handle3 = assets.Add(new TextAsset("test3"));
        var handle4 = assets.Add(new TextAsset("test4"));
        assetsContainer.SetDependencies(handle1, [handle2]);
        assetsContainer.SetDependencies(handle2, [handle3]);
        assetsContainer.SetDependencies(handle3, [handle4]);
        assetsContainer.ClearEvents();

        var events = new Events();
        assetsContainer.NotifyDependants(handle4);
        assetsContainer.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(3, assetEvents.Length);
        Assert.Equal(AssetEventType.Changed, assetEvents[0].Type);
        Assert.Equal(handle3, assetEvents[0].Handle);
        Assert.Equal(AssetEventType.Changed, assetEvents[1].Type);
        Assert.Equal(handle2, assetEvents[1].Handle);
        Assert.Equal(AssetEventType.Changed, assetEvents[2].Type);
        Assert.Equal(handle1, assetEvents[2].Handle);
    }

    [Fact]
    public void AssetsContainer_NotifyDependants_CircularDependency()
    {
        var assetsContainer = new AssetsContainer();
        var assets = assetsContainer.InitAssets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));
        var handle3 = assets.Add(new TextAsset("test3"));
        var handle4 = assets.Add(new TextAsset("test4"));
        assetsContainer.SetDependencies(handle1, [handle2]);
        assetsContainer.SetDependencies(handle2, [handle3]);
        assetsContainer.SetDependencies(handle3, [handle4]);
        assetsContainer.SetDependencies(handle4, [handle1]);
        assetsContainer.ClearEvents();

        var events = new Events();
        assetsContainer.NotifyDependants(handle4);
        assetsContainer.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(3, assetEvents.Length);
        Assert.Equal(AssetEventType.Changed, assetEvents[0].Type);
        Assert.Equal(handle3, assetEvents[0].Handle);
        Assert.Equal(AssetEventType.Changed, assetEvents[1].Type);
        Assert.Equal(handle2, assetEvents[1].Handle);
        Assert.Equal(AssetEventType.Changed, assetEvents[2].Type);
        Assert.Equal(handle1, assetEvents[2].Handle);
    }

    [Fact]
    public void AssetsContainer_DependenciesFromAttribute()
    {
        var assetsContainer = new AssetsContainer();
        var textAssetHandle = assetsContainer.AddAsset(new TextAsset("test"));
        var testAssetHandle = assetsContainer.AddAsset(new TestAsset() { Value = 1, Dependency = textAssetHandle });

        var testAssetInfo = assetsContainer.GetInfo(testAssetHandle);
        Assert.NotNull(testAssetInfo);
        Assert.NotNull(testAssetInfo.Dependencies);
        Assert.Single(testAssetInfo.Dependencies);
        Assert.Equal(textAssetHandle, testAssetInfo.Dependencies.First().As<TextAsset>());
    }

    [Fact]
    public void AssetsContainer_DependenciesFromAttribute_Nested()
    {
        var assetsContainer = new AssetsContainer();
        var textAssetHandle = assetsContainer.AddAsset(new TextAsset("test"));
        var parentAssetHandle = assetsContainer.AddAsset(new ParentTestAsset() { Value = 1, Child = new ContainsAsset() { Dependency = textAssetHandle } });

        var parentAssetInfo = assetsContainer.GetInfo(parentAssetHandle);
        Assert.NotNull(parentAssetInfo);
        Assert.NotNull(parentAssetInfo.Dependencies);
        Assert.Single(parentAssetInfo.Dependencies);
        Assert.Equal(textAssetHandle, parentAssetInfo.Dependencies.First().As<TextAsset>());
    }
}