namespace Pollus.Tests.Assets;

using System.Diagnostics;
using Pollus.ECS;
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
    public void Assets_SetDependencies()
    {
        var assets = new Assets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));
        assets.SetDependencies(handle1, [handle2]);

        var asset1Info = assets.GetInfo(handle1);
        Assert.NotNull(asset1Info);
        Assert.NotNull(asset1Info.Dependencies);
        Assert.Single(asset1Info.Dependencies);
        Assert.Equal(handle2, asset1Info.Dependencies.First().As<TextAsset>());

        var asset2Info = assets.GetInfo(handle2);
        Assert.NotNull(asset2Info);
        Assert.Null(asset2Info.Dependencies);
        Assert.Single(asset2Info.Dependents);
        Assert.Equal(handle1, asset2Info.Dependents.First().As<TextAsset>());
    }

    [Fact]
    public void Assets_NotifyDependants_SingleDependency()
    {
        var assets = new Assets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));
        assets.SetDependencies(handle1, [handle2]);
        assets.ClearEvents();

        var events = new Events();
        assets.NotifyDependants(handle2);
        assets.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(1, assetEvents.Length);
    }

    [Fact]
    public void Assets_NotifyDependants_DeepDependency()
    {
        var assets = new Assets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));
        var handle3 = assets.Add(new TextAsset("test3"));
        var handle4 = assets.Add(new TextAsset("test4"));
        assets.SetDependencies(handle1, [handle2]);
        assets.SetDependencies(handle2, [handle3]);
        assets.SetDependencies(handle3, [handle4]);
        assets.ClearEvents();

        var events = new Events();
        assets.NotifyDependants(handle4);
        assets.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(3, assetEvents.Length);
        Assert.Equal(AssetEventType.DependenciesChanged, assetEvents[0].Type);
        Assert.Equal(handle3, assetEvents[0].Handle);
        Assert.Equal(AssetEventType.DependenciesChanged, assetEvents[1].Type);
        Assert.Equal(handle2, assetEvents[1].Handle);
        Assert.Equal(AssetEventType.DependenciesChanged, assetEvents[2].Type);
        Assert.Equal(handle1, assetEvents[2].Handle);
    }

    [Fact]
    public void Assets_NotifyDependants_CircularDependency()
    {
        var assets = new Assets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));
        var handle3 = assets.Add(new TextAsset("test3"));
        var handle4 = assets.Add(new TextAsset("test4"));
        assets.SetDependencies(handle1, [handle2]);
        assets.SetDependencies(handle2, [handle3]);
        assets.SetDependencies(handle3, [handle4]);
        assets.SetDependencies(handle4, [handle1]);
        assets.ClearEvents();

        var events = new Events();
        assets.NotifyDependants(handle4);
        assets.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(3, assetEvents.Length);
        Assert.Equal(AssetEventType.DependenciesChanged, assetEvents[0].Type);
        Assert.Equal(handle3, assetEvents[0].Handle);
        Assert.Equal(AssetEventType.DependenciesChanged, assetEvents[1].Type);
        Assert.Equal(handle2, assetEvents[1].Handle);
        Assert.Equal(AssetEventType.DependenciesChanged, assetEvents[2].Type);
        Assert.Equal(handle1, assetEvents[2].Handle);
    }
}