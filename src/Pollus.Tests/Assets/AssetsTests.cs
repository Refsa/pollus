namespace Pollus.Tests.Assets;

using System.Diagnostics;
using Core.Assets;
using Pollus.ECS;
using Pollus.Assets;
using Pollus.Tests.Utils;
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
            Path = "test.txt",
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

        var handle = assetServer.Load<TextAsset>("test.txt");
        var asset = assetServer.Assets.GetAsset(handle);

        Assert.NotNull(asset);
        Assert.Equal("this is some text", asset.Content);
    }

    [Fact]
    public async Task AssetLoader_LoadAssetAsync()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle = assetServer.LoadAsync<TextAsset>("test.txt");

        var stopwatch = Stopwatch.StartNew();
        while (assetServer.Assets.GetInfo(handle)!.Status != AssetStatus.Loaded)
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
    public void AssetsContainer_NotifyDependants_SingleDependency()
    {
        var assetsContainer = new AssetsContainer();
        var assets = assetsContainer.InitAssets<TextAsset>();
        var handle1 = assets.Add(new TextAsset("test1"));
        var handle2 = assets.Add(new TextAsset("test2"));

        var info1 = (IAssetInfo)assetsContainer.GetInfo(handle1)!;
        var info2 = (IAssetInfo)assetsContainer.GetInfo(handle2)!;

        info1.SetDependencies([handle2]);
        info2.AddDependent(handle1);
        info1.Status = AssetStatus.WaitingForDependency;
        info2.Status = AssetStatus.Loaded;

        assetsContainer.ClearEvents();

        var events = new Events();
        assetsContainer.NotifyDependants(handle2);
        assetsContainer.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(1, assetEvents.Length);
        Assert.Equal(AssetEventType.Loaded, assetEvents[0].Type);
        Assert.Equal(handle1, assetEvents[0].Handle);
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

        var info1 = (IAssetInfo)assetsContainer.GetInfo(handle1)!;
        var info2 = (IAssetInfo)assetsContainer.GetInfo(handle2)!;
        var info3 = (IAssetInfo)assetsContainer.GetInfo(handle3)!;
        var info4 = (IAssetInfo)assetsContainer.GetInfo(handle4)!;

        info1.SetDependencies([handle2]);
        info2.AddDependent(handle1);
        info2.SetDependencies([handle3]);
        info3.AddDependent(handle2);
        info3.SetDependencies([handle4]);
        info4.AddDependent(handle3);

        info1.Status = AssetStatus.WaitingForDependency;
        info2.Status = AssetStatus.WaitingForDependency;
        info3.Status = AssetStatus.WaitingForDependency;
        info4.Status = AssetStatus.Loaded;

        assetsContainer.ClearEvents();

        var events = new Events();
        assetsContainer.NotifyDependants(handle4);
        assetsContainer.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(3, assetEvents.Length);
        Assert.Equal(AssetEventType.Loaded, assetEvents[0].Type);
        Assert.Equal(handle3, assetEvents[0].Handle);
        Assert.Equal(AssetEventType.Loaded, assetEvents[1].Type);
        Assert.Equal(handle2, assetEvents[1].Handle);
        Assert.Equal(AssetEventType.Loaded, assetEvents[2].Type);
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

        var info1 = (IAssetInfo)assetsContainer.GetInfo(handle1)!;
        var info2 = (IAssetInfo)assetsContainer.GetInfo(handle2)!;
        var info3 = (IAssetInfo)assetsContainer.GetInfo(handle3)!;
        var info4 = (IAssetInfo)assetsContainer.GetInfo(handle4)!;

        info1.SetDependencies([handle2]);
        info2.AddDependent(handle1);
        info2.SetDependencies([handle3]);
        info3.AddDependent(handle2);
        info3.SetDependencies([handle4]);
        info4.AddDependent(handle3);
        info4.SetDependencies([handle1]);
        info1.AddDependent(handle4);

        info1.Status = AssetStatus.WaitingForDependency;
        info2.Status = AssetStatus.WaitingForDependency;
        info3.Status = AssetStatus.WaitingForDependency;
        info4.Status = AssetStatus.Loaded;

        assetsContainer.ClearEvents();

        var events = new Events();
        assetsContainer.NotifyDependants(handle4);
        assetsContainer.FlushEvents(events);

        var assetEvents = events.GetReader<AssetEvent<TextAsset>>()!.Read();
        Assert.Equal(3, assetEvents.Length);
        Assert.Equal(AssetEventType.Loaded, assetEvents[0].Type);
        Assert.Equal(handle3, assetEvents[0].Handle);
        Assert.Equal(AssetEventType.Loaded, assetEvents[1].Type);
        Assert.Equal(handle2, assetEvents[1].Handle);
        Assert.Equal(AssetEventType.Loaded, assetEvents[2].Type);
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

    [Fact]
    public void AssetServer_Load_NonExistentFile()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle = assetServer.Load<TextAsset>("nonexistent.txt");
        Assert.True(handle == Handle<TextAsset>.Null);
    }

    [Fact]
    public void AssetServer_Load_InvalidExtension()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle = assetServer.Load("test.unknown");
        Assert.True(handle == Handle.Null);
    }

    [Fact]
    public void AssetServer_Load_AlreadyLoaded()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle1 = assetServer.Load<TextAsset>("test.txt");
        var handle2 = assetServer.Load<TextAsset>("test.txt", reload: false);

        Assert.Equal(handle1, handle2);
    }

    [Fact]
    public void AssetServer_Load_Reload()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle1 = assetServer.Load<TextAsset>("test.txt");
        var asset1 = assetServer.Assets.GetAsset(handle1);
        var handle2 = assetServer.Load<TextAsset>("test.txt", reload: true);
        var asset2 = assetServer.Assets.GetAsset(handle2);

        Assert.Equal(handle1, handle2);
        Assert.NotNull(asset1);
        Assert.NotNull(asset2);
        Assert.Equal("this is some text", asset1.Content);
        Assert.Equal("this is some text", asset2.Content);
    }

    [Fact]
    public void AssetServer_LoadAsync_AlreadyLoaded()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle1 = assetServer.Load<TextAsset>("test.txt");
        assetServer.FlushLoading();

        var handle2 = assetServer.LoadAsync<TextAsset>("test.txt", reload: false);

        Assert.Equal(handle1, handle2);
        Assert.Equal(0, assetServer.PendingLoads);
    }

    [Fact]
    public void AssetServer_LoadAsync_Reload()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle1 = assetServer.Load<TextAsset>("test.txt");
        assetServer.FlushLoading();

        var handle2 = assetServer.LoadAsync<TextAsset>("test.txt", reload: true);
        assetServer.FlushLoading();

        Assert.Equal(handle1, handle2);
    }

    [Fact]
    public void AssetServer_Update_FailedLoad()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("test.txt", "test"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>();

        var handle = assetServer.LoadAsync<TextAsset>("test.txt");

        var info = assetServer.Assets.GetInfo(handle);
        Assert.NotNull(info);

        assetServer.Assets.SetFailed(handle);

        assetServer.Update();

        var finalInfo = assetServer.Assets.GetInfo(handle);
        Assert.NotNull(finalInfo);
        Assert.Equal(AssetStatus.Failed, finalInfo.Status);
    }

    [Fact]
    public void AssetServer_Queue_ValidPath()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        assetServer.InitAssets<TextAsset>();
        var handle = assetServer.Queue<TextAsset>("test.txt");

        Assert.False(handle == Handle<TextAsset>.Null);
        var info = assetServer.Assets.GetInfo(handle);
        Assert.NotNull(info);
        Assert.Equal(AssetStatus.Initialized, info.Status);
    }

    [Fact]
    public void AssetServer_Queue_InvalidExtension()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        assetServer.InitAssets<TextAsset>();
        var handle = assetServer.Queue("test.unknown");

        Assert.True(handle == Handle.Null);
    }

    [Fact]
    public void AssetServer_Queue_UninitializedAssetType()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        var handle = assetServer.Queue("test.txt");

        Assert.True(handle == Handle.Null);
    }

    [Fact]
    public async Task AssetServer_FlushQueue()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("test1.txt", "test1"u8.ToArray());
        testIO.AddFile("test2.txt", "test2"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>();
        assetServer.InitAssets<TextAsset>();

        var handle1 = assetServer.Queue("test1.txt");
        var handle2 = assetServer.Queue("test2.txt");

        Assert.False(handle1 == Handle.Null);
        Assert.False(handle2 == Handle.Null);

        await Task.Delay(400);
        assetServer.FlushQueue();

        Assert.Equal(2, assetServer.PendingLoads);
        var info1 = assetServer.Assets.GetInfo(handle1);
        var info2 = assetServer.Assets.GetInfo(handle2);

        Assert.NotNull(info1);
        Assert.NotNull(info2);
    }

    [Fact]
    public void AssetServer_FlushLoading()
    {
        var testIO = new TestAssetIO();
        testIO.AddFile("test1.txt", "test1"u8.ToArray());
        testIO.AddFile("test2.txt", "test2"u8.ToArray());

        var assetServer = new AssetServer(testIO)
            .AddLoader<TextAssetLoader>();

        var handle1 = assetServer.LoadAsync<TextAsset>("test1.txt");
        var handle2 = assetServer.LoadAsync<TextAsset>("test2.txt");

        assetServer.FlushLoading();

        var asset1 = assetServer.Assets.GetAsset(handle1);
        var asset2 = assetServer.Assets.GetAsset(handle2);

        Assert.NotNull(asset1);
        Assert.NotNull(asset2);
        Assert.Equal("test1", asset1.Content);
        Assert.Equal("test2", asset2.Content);
    }

    [Fact]
    public void AssetServer_AddLoader_WithInstance()
    {
        var loader = new TextAssetLoader();
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader(loader);

        var handle = assetServer.Load<TextAsset>("test.txt");
        var asset = assetServer.Assets.GetAsset(handle);

        Assert.NotNull(asset);
        Assert.Equal("this is some text", asset.Content);
    }

    [Fact]
    public void AssetServer_GetAssets_Generic()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        assetServer.InitAssets<TextAsset>();
        var assets = assetServer.GetAssets<TextAsset>();

        Assert.NotNull(assets);
    }

    [Fact]
    public void AssetServer_GetAssets_TypeID()
    {
        var assetServer = new AssetServer(new FileAssetIO("Assets/TestFiles"))
            .AddLoader<TextAssetLoader>();

        assetServer.InitAssets<TextAsset>();
        var typeId = TypeLookup.ID<TextAsset>();
        var assets = assetServer.GetAssets(typeId);

        Assert.NotNull(assets);
        Assert.True(typeId == assets.AssetType);
    }
}