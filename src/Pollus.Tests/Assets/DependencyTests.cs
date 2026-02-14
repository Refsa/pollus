namespace Pollus.Tests.Assets;

using Pollus.Assets;
using Pollus.Core.Assets;
using Pollus.Utils;

public class DependencyTests
{
    class TestAsset : IAsset
    {
        public HashSet<Handle> Dependencies { get; set; } = [];
    }

    [Fact]
    public void TestDependencyWaiting()
    {
        var container = new AssetsContainer();
        var storage = container.InitAssets<TestAsset>();
        
        var depHandle = storage.Initialize(new AssetPath("dep"));
        
        var dependentAsset = new TestAsset();
        dependentAsset.Dependencies.Add(depHandle);
        
        var dependentHandle = storage.Add(dependentAsset, new AssetPath("dependent"));
        
        var status = container.GetInfo(dependentHandle)!.Status;
        Assert.Equal(AssetStatus.WaitingForDependency, status);
        
        storage.Set(depHandle, new TestAsset());
        
        status = container.GetInfo(dependentHandle)!.Status;
        Assert.Equal(AssetStatus.Loaded, status);
    }
}

