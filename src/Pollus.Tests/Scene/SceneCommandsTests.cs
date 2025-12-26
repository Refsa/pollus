namespace Pollus.Tests.Scene;

using Engine;
using Engine.Assets;
using Pollus.ECS;
using Utils;

public class SceneCommandsTests
{
    static (World world, TestAssetIO assetIO) CreateWorld()
    {
        var assetIO = new TestAssetIO("assets");
        var assetServer = new AssetServer(assetIO);
        var world = WorldBuilder.Default
            .AddResource(assetServer)
            .AddPlugin(new ScenePlugin()
            {
                TypesVersion = 1,
            })
            .Build();
        return (world, assetIO);
    }

    [Fact]
    public void SceneCommands_SpawnScene_SingleEntity_SingleComponent()
    {
        var (world, assetIO) = CreateWorld();
        assetIO.AddFile("assets/scene.scene", $@"
{{
    ""Types"": {{
        ""TestComponent"": ""{typeof(TestComponent).AssemblyQualifiedName}""
    }},
    ""Entities"": [
        {{
            ""ID"": 1,
            ""Name"": ""Entity1"",
            ""Components"": {{
                ""TestComponent"": {{
                    ""Value"": 42
                }}
            }}
        }}
    ]
}}
".ToBytes());

        var scene = world.Resources.Get<AssetServer>().Load<Scene>("assets/scene.scene");

        var commands = world.GetCommands();
        var root = commands.SpawnScene(scene);
        world.Update();

        {
            Assert.Equal(2, world.Store.EntityCount);
            
            var rootRef = world.GetEntityRef(root.Entity);
            var childRef = world.GetEntityRef(rootRef.Get<Parent>().FirstChild);
            var component = childRef.Get<TestComponent>();
            Assert.Equal(42, component.Value);
        }

        world.Dispose();
    }
}