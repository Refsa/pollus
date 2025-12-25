namespace Pollus.Tests.Scene;

using System.Runtime.InteropServices;
using System.Text;
using Engine;
using Engine.Assets;
using Pollus.Utils;
using Utils;

public class SceneAssetLoaderTests
{
    [Fact]
    public void Parse_SceneWithChildScene()
    {
        var parentSceneJson =
            $$"""
              {
                "types": {
                  "SceneRef": "{{typeof(SceneRef).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "scene": "path/to/child.scene"
                  }
                ]
              }
              """;

        var childSceneJson =
            $$"""
              {
                "types": {
                  "TestComponent": "{{typeof(TestComponent).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "TestComponent": {
                        "Value": 10
                      }
                    }
                  }
                ]
              }
              """;

        var assetIO = new TestAssetIO("assets")
            .AddFile("path/to/parent.scene", Encoding.UTF8.GetBytes(parentSceneJson))
            .AddFile("path/to/child.scene", Encoding.UTF8.GetBytes(childSceneJson));

        var server = new AssetServer(assetIO);
        server.AddLoader<SceneAssetLoader>(new()
        {
            SceneSerializer = new SceneSerializer(1, 1),
        });

        var parentSceneHandle = server.Load<Scene>("path/to/parent.scene");
        server.FlushLoading();

        var parentScene = server.GetAssets<Scene>().Get(parentSceneHandle);
        Assert.NotNull(parentScene);
        Assert.Single(parentScene.Entities);
        Assert.Equal("Entity1", parentScene.Entities[0].Name);
        Assert.NotEqual(Handle<Scene>.Null, parentScene.Entities[0].Scene);

        Assert.Single(parentScene.Scenes);
        var childScene = server.GetAssets<Scene>().Get(parentScene.Scenes.Values.First());
        Assert.NotNull(childScene);
        Assert.Single(childScene.Entities);
        Assert.Equal("Entity1", childScene.Entities[0].Name);
        var testComponent = MemoryMarshal.AsRef<TestComponent>(childScene.Entities[0].Components[0].Data);
        Assert.Equal(10, testComponent.Value);

        var parentAssetInfo = server.GetAssets<Scene>().GetInfo(parentSceneHandle);
        Assert.NotNull(parentAssetInfo);
        Assert.NotNull(parentAssetInfo.Dependencies);
        Assert.Single(parentAssetInfo.Dependencies);
        Assert.Collection(parentAssetInfo.Dependencies, handle => Assert.Equal(parentScene.Scenes.Values.First(), handle.As<Scene>()));
    }
}