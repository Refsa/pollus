namespace Pollus.Tests.Assets;

using System.Text;
using Pollus.Assets;
using Pollus.Core.Assets;
using Pollus.Tests.Utils;
using Pollus.Utils;

[Asset]
partial class PreloadTestAsset
{
    public string Value { get; set; } = "";
    public string DependencyValue { get; set; } = "";
    public Handle<TextAsset> Dep { get; set; }
}

class PreloadState
{
    public string Value { get; set; } = "";
    public Handle<TextAsset> DepHandle { get; set; }
}

class PreloadTestAssetLoader : AssetLoader<PreloadTestAsset, PreloadState>
{
    public override string[] Extensions => [".preload"];

    protected override PreloadState Preprocess(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        var text = Encoding.UTF8.GetString(data);
        var parts = text.Split('|');
        var depHandle = context.LoadDependency<TextAsset>(parts[1]);
        return new PreloadState { Value = parts[0], DepHandle = depHandle };
    }

    protected override void Load(PreloadState state, ref LoadContext context)
    {
        var depAsset = context.AssetServer.Assets.GetAsset(state.DepHandle);
        context.SetAsset(new PreloadTestAsset
        {
            Value = state.Value,
            DependencyValue = depAsset?.Content ?? "",
            Dep = state.DepHandle,
        });
    }
}

public class PreloadableAssetLoaderTests
{
    [Fact]
    public void Preload_SetsStateAndDependencies()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("dep.txt", "dep_value"u8.ToArray());
        testIO.AddFile("test.preload", "my_value|dep.txt"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>()
            .AddLoader<PreloadTestAssetLoader>();

        var loader = new PreloadTestAssetLoader();
        var loadContext = new LoadContext()
        {
            Path = "test.preload",
            FileName = "test",
            Handle = assetServer.InitAssets<PreloadTestAsset>().Initialize("test.preload"),
            AssetServer = assetServer,
            Loader = loader,
        };

        loader.Load("my_value|dep.txt"u8, ref loadContext);

        Assert.Equal(AssetLoadStatus.Preprocess, loadContext.Status);
        Assert.NotNull(loadContext.State);
        Assert.IsType<PreloadState>(loadContext.State);
        Assert.NotNull(loadContext.Dependencies);
        Assert.Single(loadContext.Dependencies);
    }

    [Fact]
    public void AsyncPipeline_CompletesFullTwoPhase()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("dep.txt", "dep_value"u8.ToArray());
        testIO.AddFile("test.preload", "my_value|dep.txt"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>()
            .AddLoader<PreloadTestAssetLoader>();

        assetServer.InitAssets<TextAsset>();
        assetServer.InitAssets<PreloadTestAsset>();

        var handle = assetServer.LoadAsync<PreloadTestAsset>("test.preload");
        Assert.False(handle == Handle<PreloadTestAsset>.Null);

        assetServer.FlushLoading();

        var asset = assetServer.Assets.GetAsset(handle);
        Assert.NotNull(asset);
        Assert.Equal("my_value", asset.Value);
        Assert.Equal("dep_value", asset.DependencyValue);
    }

    [Fact]
    public void StepThroughUpdate_AssetCompletesWhenDepsReady()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("dep.txt", "dep_value"u8.ToArray());
        testIO.AddFile("test.preload", "my_value|dep.txt"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>()
            .AddLoader<PreloadTestAssetLoader>();

        assetServer.InitAssets<TextAsset>();
        assetServer.InitAssets<PreloadTestAsset>();

        var handle = assetServer.LoadAsync<PreloadTestAsset>("test.preload");

        // First update: file I/O completes, Preload runs, dep is queued async
        assetServer.Update();

        // Asset should not be loaded yet - dep not ready
        var asset = assetServer.Assets.GetAsset(handle);
        Assert.Null(asset);
        Assert.True(assetServer.PendingLoads > 0);

        // Keep updating until everything resolves
        assetServer.FlushLoading();

        asset = assetServer.Assets.GetAsset(handle);
        Assert.NotNull(asset);
        Assert.Equal("my_value", asset.Value);
        Assert.Equal("dep_value", asset.DependencyValue);
    }

    [Fact]
    public void NonPreloadableLoader_WorksIdentically()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("test.txt", "hello"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>();

        var handle = assetServer.LoadAsync<TextAsset>("test.txt");
        assetServer.FlushLoading();

        var asset = assetServer.Assets.GetAsset(handle);
        Assert.NotNull(asset);
        Assert.Equal("hello", asset.Content);
    }

    [Fact]
    public void FailedDependency_PropagatesFailure()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("dep.txt", "dep_value"u8.ToArray());
        testIO.AddFile("test.preload", "my_value|dep.txt"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>()
            .AddLoader<PreloadTestAssetLoader>();

        assetServer.InitAssets<TextAsset>();
        assetServer.InitAssets<PreloadTestAsset>();

        // Pre-get the dep handle so we can fail it later
        var depHandle = assetServer.Assets.GetHandle<TextAsset>("dep.txt");

        var handle = assetServer.LoadAsync<PreloadTestAsset>("test.preload");

        // First update: preload runs, dep queued
        assetServer.Update();

        // Fail the dependency
        assetServer.Assets.SetFailed(depHandle);

        // Next update should propagate failure
        assetServer.Update();

        var finalInfo = assetServer.Assets.GetInfo(handle);
        Assert.NotNull(finalInfo);
        Assert.Equal(AssetStatus.Failed, finalInfo.Status);
    }

    [Fact]
    public void SyncLoad_PreloadableWithDepsAlreadyLoaded()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("dep.txt", "dep_value"u8.ToArray());
        testIO.AddFile("test.preload", "my_value|dep.txt"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>()
            .AddLoader<PreloadTestAssetLoader>();

        // Pre-load the dependency synchronously
        var depHandle = assetServer.Load<TextAsset>("dep.txt");
        Assert.False(depHandle == Handle<TextAsset>.Null);

        assetServer.InitAssets<PreloadTestAsset>();

        var handle = assetServer.Load<PreloadTestAsset>("test.preload");
        Assert.False(handle == Handle<PreloadTestAsset>.Null);

        var asset = assetServer.Assets.GetAsset(handle);
        Assert.NotNull(asset);
        Assert.Equal("my_value", asset.Value);
        Assert.Equal("dep_value", asset.DependencyValue);
    }
}
