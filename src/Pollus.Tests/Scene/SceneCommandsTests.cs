namespace Pollus.Tests.Scene;

using Engine;
using Engine.Assets;
using Engine.Transform;
using Pollus.ECS;
using Utils;

public partial struct RequiredA : IComponent, IDefault<RequiredA>
{
    public static RequiredA Default { get; } = new() { Tag = 1 };
    public int Tag;
}

public partial struct RequiredB : IComponent, IDefault<RequiredB>
{
    public static RequiredB Default { get; } = new() { Tag = 2 };
    public int Tag;
}

[Required<RequiredA>]
public partial struct HasOneRequired : IComponent
{
    public int Value;
}

[Required<RequiredA>, Required<RequiredB>]
public partial struct HasTwoRequired : IComponent
{
    public int Value;
}

[Required<RequiredA>]
public partial struct AlsoRequiresA : IComponent
{
    public int Data;
}

[Required<Transform2D>, Required<GlobalTransform>]
public partial struct NeedsTransforms : IComponent
{
    public int Value;
}

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
    public void SpawnScene_SingleEntity_SingleComponent()
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

        Assert.Equal(2, world.Store.EntityCount);

        var rootRef = world.GetEntityRef(root.Entity);
        var childRef = world.GetEntityRef(rootRef.Get<Parent>().FirstChild);
        var component = childRef.Get<TestComponent>();
        Assert.Equal(42, component.Value);

        world.Dispose();
    }

    [Fact]
    public void SpawnScene_AddsRequiredComponents_WhenMissing()
    {
        var (world, assetIO) = CreateWorld();
        assetIO.AddFile("assets/scene.scene", $@"
{{
    ""Types"": {{
        ""HasOneRequired"": ""{typeof(HasOneRequired).AssemblyQualifiedName}""
    }},
    ""Entities"": [
        {{
            ""Name"": ""Entity1"",
            ""Components"": {{
                ""HasOneRequired"": {{
                    ""Value"": 10
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

        var rootRef = world.GetEntityRef(root.Entity);
        var childRef = world.GetEntityRef(rootRef.Get<Parent>().FirstChild);

        Assert.True(childRef.Has<HasOneRequired>());
        Assert.True(childRef.Has<RequiredA>());
        Assert.Equal(10, childRef.Get<HasOneRequired>().Value);
        Assert.Equal(RequiredA.Default.Tag, childRef.Get<RequiredA>().Tag);

        world.Dispose();
    }

    [Fact]
    public void SpawnScene_AddsMultipleRequiredComponents()
    {
        var (world, assetIO) = CreateWorld();
        assetIO.AddFile("assets/scene.scene", $@"
{{
    ""Types"": {{
        ""HasTwoRequired"": ""{typeof(HasTwoRequired).AssemblyQualifiedName}""
    }},
    ""Entities"": [
        {{
            ""Name"": ""Entity1"",
            ""Components"": {{
                ""HasTwoRequired"": {{
                    ""Value"": 5
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

        var rootRef = world.GetEntityRef(root.Entity);
        var childRef = world.GetEntityRef(rootRef.Get<Parent>().FirstChild);

        Assert.True(childRef.Has<HasTwoRequired>());
        Assert.True(childRef.Has<RequiredA>());
        Assert.True(childRef.Has<RequiredB>());
        Assert.Equal(5, childRef.Get<HasTwoRequired>().Value);
        Assert.Equal(RequiredA.Default.Tag, childRef.Get<RequiredA>().Tag);
        Assert.Equal(RequiredB.Default.Tag, childRef.Get<RequiredB>().Tag);

        world.Dispose();
    }

    [Fact]
    public void SpawnScene_DoesNotOverrideExplicitComponent()
    {
        var (world, assetIO) = CreateWorld();
        assetIO.AddFile("assets/scene.scene", $@"
{{
    ""Types"": {{
        ""HasOneRequired"": ""{typeof(HasOneRequired).AssemblyQualifiedName}"",
        ""RequiredA"": ""{typeof(RequiredA).AssemblyQualifiedName}""
    }},
    ""Entities"": [
        {{
            ""Name"": ""Entity1"",
            ""Components"": {{
                ""RequiredA"": {{
                    ""Tag"": 99
                }},
                ""HasOneRequired"": {{
                    ""Value"": 10
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

        var rootRef = world.GetEntityRef(root.Entity);
        var childRef = world.GetEntityRef(rootRef.Get<Parent>().FirstChild);

        Assert.True(childRef.Has<RequiredA>());
        Assert.Equal(99, childRef.Get<RequiredA>().Tag);

        world.Dispose();
    }

    [Fact]
    public void SpawnScene_SharedRequired_AddedOnce()
    {
        var (world, assetIO) = CreateWorld();
        assetIO.AddFile("assets/scene.scene", $@"
{{
    ""Types"": {{
        ""HasOneRequired"": ""{typeof(HasOneRequired).AssemblyQualifiedName}"",
        ""AlsoRequiresA"": ""{typeof(AlsoRequiresA).AssemblyQualifiedName}""
    }},
    ""Entities"": [
        {{
            ""Name"": ""Entity1"",
            ""Components"": {{
                ""HasOneRequired"": {{
                    ""Value"": 1
                }},
                ""AlsoRequiresA"": {{
                    ""Data"": 2
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

        var rootRef = world.GetEntityRef(root.Entity);
        var childRef = world.GetEntityRef(rootRef.Get<Parent>().FirstChild);

        Assert.True(childRef.Has<HasOneRequired>());
        Assert.True(childRef.Has<AlsoRequiresA>());
        Assert.True(childRef.Has<RequiredA>());
        Assert.Equal(1, childRef.Get<HasOneRequired>().Value);
        Assert.Equal(2, childRef.Get<AlsoRequiresA>().Data);

        world.Dispose();
    }

    [Fact]
    public void SpawnScene_AddsRequiredTransformComponents()
    {
        var (world, assetIO) = CreateWorld();
        assetIO.AddFile("assets/scene.scene", $@"
{{
    ""Types"": {{
        ""NeedsTransforms"": ""{typeof(NeedsTransforms).AssemblyQualifiedName}""
    }},
    ""Entities"": [
        {{
            ""Name"": ""TransformEntity"",
            ""Components"": {{
                ""NeedsTransforms"": {{
                    ""Value"": 7
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

        var rootRef = world.GetEntityRef(root.Entity);
        var childRef = world.GetEntityRef(rootRef.Get<Parent>().FirstChild);

        Assert.True(childRef.Has<NeedsTransforms>());
        Assert.True(childRef.Has<Transform2D>());
        Assert.True(childRef.Has<GlobalTransform>());
        Assert.Equal(7, childRef.Get<NeedsTransforms>().Value);

        world.Dispose();
    }

    [Fact]
    public void SpawnScene_MultipleEntities_EachGetsRequiredComponents()
    {
        var (world, assetIO) = CreateWorld();
        assetIO.AddFile("assets/scene.scene", $@"
{{
    ""Types"": {{
        ""HasOneRequired"": ""{typeof(HasOneRequired).AssemblyQualifiedName}"",
        ""TestComponent"": ""{typeof(TestComponent).AssemblyQualifiedName}""
    }},
    ""Entities"": [
        {{
            ""Name"": ""WithRequired"",
            ""Components"": {{
                ""HasOneRequired"": {{ ""Value"": 1 }}
            }}
        }},
        {{
            ""Name"": ""WithoutRequired"",
            ""Components"": {{
                ""TestComponent"": {{ ""Value"": 2 }}
            }}
        }}
    ]
}}
".ToBytes());

        var scene = world.Resources.Get<AssetServer>().Load<Scene>("assets/scene.scene");
        var commands = world.GetCommands();
        var root = commands.SpawnScene(scene);
        world.Update();

        // 1 root + 2 children
        Assert.Equal(3, world.Store.EntityCount);

        var rootRef = world.GetEntityRef(root.Entity);
        var firstChild = rootRef.Get<Parent>().FirstChild;
        var firstChildRef = world.GetEntityRef(firstChild);

        Assert.True(firstChildRef.Has<HasOneRequired>());
        Assert.True(firstChildRef.Has<RequiredA>());

        var secondChild = world.GetEntityRef(firstChild).Get<Child>().NextSibling;
        var secondChildRef = world.GetEntityRef(secondChild);

        Assert.True(secondChildRef.Has<TestComponent>());
        Assert.False(secondChildRef.Has<RequiredA>());

        world.Dispose();
    }
}
