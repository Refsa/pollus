// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Pollus.Tests.Scene;

using System.Text.Json;
using Core.Assets;
using Pollus.Serialization;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Assets;
using Pollus.Utils;
using Xunit;
using Pollus.Core.Serialization;
using Pollus.Tests.Utils;

public class SceneWriterTests
{
    World CreateWorld(TestAssetIO? assetIO = null)
    {
        var world = new World();
        world.Resources.Add(new AssetServer(assetIO ?? new TestAssetIO("assets")));
        return world;
    }

    JsonDocument ParseJson(byte[] data)
    {
        return JsonDocument.Parse(data);
    }

    [Fact]
    public void Write_Types()
    {
        using var world = CreateWorld();
        var entity = world.Spawn(Entity.With(new TestComponent { Value = 10 }));

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var types = root.GetProperty("Types");
        Assert.True(types.TryGetProperty("TestComponent", out var typeName));
        Assert.Equal(typeof(TestComponent).AssemblyQualifiedName, typeName.GetString());
    }

    [Fact]
    public void Write_Entity_Simple()
    {
        using var world = CreateWorld();
        var entity = world.Spawn();

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entities = root.GetProperty("Entities");
        Assert.Equal(1, entities.GetArrayLength());

        var entityData = entities[0];
        Assert.True(entityData.TryGetProperty("ID", out var id));
        Assert.Equal(entity.ID, id.GetInt32());
    }

    [Fact]
    public void Write_Entity_EmptyComponent()
    {
        using var world = CreateWorld();
        var entity = world.Spawn(Entity.With(new TestEmptyComponent()));

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];

        var components = entityData.GetProperty("Components");
        var comp = components.GetProperty("TestEmptyComponent");
        Assert.Equal(JsonValueKind.Object, comp.ValueKind);
    }

    [Fact]
    public void Write_Entity_WithComponent()
    {
        using var world = CreateWorld();
        var entity = world.Spawn(Entity.With(new TestComponent { Value = 42 }));

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var components = entityData.GetProperty("Components");

        Assert.True(components.TryGetProperty("TestComponent", out var comp));
        Assert.Equal(42, comp.GetProperty("Value").GetInt32());
    }

    [Fact]
    public void Write_Entity_WithInlineObject()
    {
        using var world = CreateWorld();
        var entity = world.Spawn(Entity.With(new TestComplexComponent { Position = new Vec2 { X = 10.5f, Y = 20.5f, Ignore = 42 } }));

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var components = entityData.GetProperty("Components");

        Assert.True(components.TryGetProperty("TestComplexComponent", out var comp));
        var pos = comp.GetProperty("Position");

        Assert.Equal(10.5f, pos.GetProperty("X").GetSingle());
        Assert.Equal(20.5f, pos.GetProperty("Y").GetSingle());
        Assert.False(pos.TryGetProperty("Ignore", out _));
    }

    [Fact]
    public void Write_NestedChildren()
    {
        using var world = CreateWorld();
        var parent = world.Spawn();
        var child1 = world.Spawn();
        var child2 = world.Spawn();
        var grandChild = world.Spawn();

        new AddChildCommand { Parent = parent, Child = child1 }.Execute(world);
        new AddChildCommand { Parent = parent, Child = child2 }.Execute(world);
        new AddChildCommand { Parent = child2, Child = grandChild }.Execute(world);

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, parent);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];

        Assert.Equal(parent.ID, entityData.GetProperty("ID").GetInt32());
        Assert.True(entityData.TryGetProperty("Children", out var children));

        Assert.Equal(2, children.GetArrayLength());

        var c1 = children[0];
        var c2 = children[1];

        var c1ID = c1.GetProperty("ID").GetInt32();
        var c2ID = c2.GetProperty("ID").GetInt32();

        Assert.Contains(c1ID, new HashSet<int> { child1.ID, child2.ID });
        Assert.Contains(c2ID, new HashSet<int> { child1.ID, child2.ID });

        var child2Data = c1ID == child2.ID ? c1 : c2;
        Assert.True(child2Data.TryGetProperty("Children", out var grandChildren));
        Assert.Equal(1, grandChildren.GetArrayLength());
        Assert.Equal(grandChild.ID, grandChildren[0].GetProperty("ID").GetInt32());
    }

    [Fact]
    public void Write_WithTypedHandle()
    {
        var assetIO = new TestAssetIO("assets");
        assetIO.AddFile("path/to/asset.txt", "this is some asset"u8.ToArray());

        using var world = CreateWorld(assetIO);
        world.Resources.Get<AssetServer>().AddLoader<TextAssetLoader>();
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<TextAsset>());

        var handle = world.Resources.Get<AssetServer>().Load<TextAsset>("path/to/asset.txt");
        var entity = world.Spawn(Entity.With(new TestComponentWithHandle { Value = 10, AssetHandle = handle }));

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var components = entityData.GetProperty("Components");

        Assert.True(components.TryGetProperty("TestComponentWithHandle", out var comp));
        Assert.Equal(10, comp.GetProperty("Value").GetInt32());

        var assetHandle = comp.GetProperty("AssetHandle").GetProperty("$path");
        Assert.Equal("path/to/asset.txt", assetHandle.GetString());
    }

    [Fact]
    public void Write_WithTypedHandle_Hierarchy()
    {
        var assetIO = new TestAssetIO("assets")
            .AddFile("path/to/child1.txt", "this is child 1 asset"u8.ToArray())
            .AddFile("path/to/child2.txt", "this is child 2 asset"u8.ToArray());

        using var world = CreateWorld(assetIO);
        var server = world.Resources.Get<AssetServer>();
        server.AddLoader<TextAssetLoader>();
        server.InitAssets<TextAsset>();

        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<TextAsset>());
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<RootAsset>());
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<ChildAsset>());

        var child1 = server.Load<TextAsset>("path/to/child1.txt");
        var child2 = server.Load<TextAsset>("path/to/child2.txt");

        var child1Asset = server.GetAssets<ChildAsset>().Add(new ChildAsset { Text = child1 });
        var child2Asset = server.GetAssets<ChildAsset>().Add(new ChildAsset { Text = child2 });
        var rootAsset = server.GetAssets<RootAsset>().Add(new RootAsset { Child1 = child1Asset, Child2 = child2Asset });

        var entity = world.Spawn(Entity.With(new ComplexHandleComponent { Root = rootAsset }));

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var components = entityData.GetProperty("Components");

        Assert.True(components.TryGetProperty("ComplexHandleComponent", out var comp));

        var rootProp = comp.GetProperty("Root");
        Assert.Equal(JsonValueKind.Object, rootProp.ValueKind);
    }

    [Fact]
    public void Write_Entity_MultipleComponents()
    {
        using var world = CreateWorld();
        var entity = world.Spawn(Entity
            .With(new TestComponent { Value = 0 })
            .With(new TestComplexComponent { Position = new Vec2 { X = 10, Y = 20 } }));

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var components = entityData.GetProperty("Components");

        Assert.True(components.TryGetProperty("TestComponent", out var c1));
        Assert.Equal(0, c1.GetProperty("Value").GetInt32());

        Assert.True(components.TryGetProperty("TestComplexComponent", out var c2));
        var pos = c2.GetProperty("Position");
        Assert.Equal(10, pos.GetProperty("X").GetInt32());
        Assert.Equal(20, pos.GetProperty("Y").GetInt32());
    }

    [Fact]
    public void Write_WithEnum()
    {
        using var world = CreateWorld();
        var entity = world.Spawn(Entity.With(new TestComponentWithEnum { EnumValue = TestEnum.One }));

        using var writer = new SceneWriter(new()
        {
            WriteRoot = true,
        });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var components = entityData.GetProperty("Components");

        Assert.True(components.TryGetProperty("TestComponentWithEnum", out var comp));

        var val = comp.GetProperty("EnumValue");
        if (val.ValueKind == JsonValueKind.String)
            Assert.Equal("One", val.GetString());
        else
            Assert.Equal(1, val.GetInt32());
    }

    [Fact]
    public void Write_Entity_WithSceneRef_DontWriteSubScenes()
    {
        var assetIO = new TestAssetIO(
            ("path/to/scene.scene", ""u8.ToArray())
        );
        var assetServer = new AssetServer(assetIO);
        var sceneHandle = assetServer.Assets.AddAsset(Scene.Empty, new AssetPath("path/to/scene.scene"));

        using var world = CreateWorld();
        world.Resources.Add(assetServer);

        var commands = world.GetCommands();
        var parent = commands.Spawn()
            .AddChildren([
                commands.Spawn(Entity.With(new TestComponent() { Value = 10 })).Entity,
                commands.Spawn(Entity.With(new TestComponent() { Value = 20 })).Entity,
                commands.Spawn(Entity.With(new SceneRef() { Scene = sceneHandle }))
                    .AddChildren([
                        commands.Spawn(Entity.With(new TestComponent() { Value = 30 })).Entity,
                        commands.Spawn(Entity.With(new TestComponent() { Value = 40 })).Entity,
                        commands.Spawn(Entity.With(new TestComponent() { Value = 50 })).Entity,
                    ])
                    .Entity,
            ]);
        world.Update();

        var writer = new SceneWriter(new()
        {
            WriteSubScenes = false,
            WriteRoot = false,
        });

        var bytes = writer.Write(world, parent.Entity);
        var json = ParseJson(bytes);

        var entities = json.RootElement.GetProperty("Entities").Deserialize<List<SceneFileData.EntityData>>();
        Assert.NotNull(entities);
        Assert.Equal(3, entities.Count);

        Assert.Collection(entities, [
            entity => Assert.Null(entity.Children),
            entity => Assert.Null(entity.Children),
            entity => Assert.Null(entity.Children),
        ]);

        Assert.Collection(entities, [
            entity =>
            {
                Assert.NotNull(entity.Components);
                Assert.Single(entity.Components);
            },
            entity =>
            {
                Assert.NotNull(entity.Components);
                Assert.Single(entity.Components);
            },
            entity => Assert.Null(entity.Components),
        ]);

        var lastChild = entities[^1];
        Assert.Equal("path/to/scene.scene", lastChild.Scene);
    }

    [Fact]
    public void Write_Entity_WithSceneRef_WriteSubScenes()
    {
        var assetIO = new TestAssetIO(
            ("path/to/scene.scene", ""u8.ToArray())
        );
        var assetServer = new AssetServer(assetIO);
        assetServer.Assets.AddAsset(Scene.Empty, new AssetPath("path/to/scene.scene"));

        using var world = CreateWorld();
        world.Resources.Add(assetServer);

        var commands = world.GetCommands();
        var parent = commands.Spawn()
            .AddChildren([
                commands.Spawn(Entity.With(new TestComponent() { Value = 10 })).Entity,
                commands.Spawn(Entity.With(new TestComponent() { Value = 20 })).Entity,
                commands.Spawn(Entity.With(new SceneRef() { Scene = new Handle<Scene>(0) }))
                    .AddChildren([
                        commands.Spawn(Entity.With(new TestComponent() { Value = 30 })).Entity,
                        commands.Spawn(Entity.With(new TestComponent() { Value = 40 })).Entity,
                        commands.Spawn(Entity.With(new TestComponent() { Value = 50 })).Entity,
                    ])
                    .Entity,
            ]);
        world.Update();

        var writer = new SceneWriter(new()
        {
            WriteSubScenes = true,
            WriteRoot = false,
        });

        var bytes = writer.Write(world, parent.Entity);
        var json = ParseJson(bytes);

        var entities = json.RootElement.GetProperty("Entities").Deserialize<List<SceneFileData.EntityData>>();
        Assert.NotNull(entities);
        Assert.Equal(3, entities.Count);

        Assert.Collection(entities, [
            entity =>
            {
                Assert.Null(entity.Children);

                Assert.NotNull(entity.Components);
                Assert.Single(entity.Components);
            },
            entity =>
            {
                Assert.Null(entity.Children);

                Assert.NotNull(entity.Components);
                Assert.Single(entity.Components);
            },
            entity =>
            {
                Assert.NotNull(entity.Components);
                Assert.Empty(entity.Components);

                Assert.NotNull(entity.Children);
                Assert.Equal(3, entity.Children.Count);

                Assert.Collection(entity.Children, [
                    child => Assert.Single(child.Components!),
                    child => Assert.Single(child.Components!),
                    child => Assert.Single(child.Components!),
                ]);
            },
        ]);
    }

    [Fact]
    public void Write_WithUntypedHandle_Inline()
    {
        using var world = CreateWorld();
        var server = world.Resources.Get<AssetServer>();
        server.InitAssets<SimpleTestAsset>();

        Handle<SimpleTestAsset> assetHandle = server.GetAssets<SimpleTestAsset>()
            .Add(new SimpleTestAsset { Data = 42 });

        var entity = world.Spawn(Entity.With(new TestComponentWithUntypedHandle
        {
            UntypedHandle = assetHandle,
            Value = 10,
        }));

        using var writer = new SceneWriter(new() { WriteRoot = true });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var components = entityData.GetProperty("Components");
        var comp = components.GetProperty("TestComponentWithUntypedHandle");

        Assert.Equal(10, comp.GetProperty("Value").GetInt32());

        var handleProp = comp.GetProperty("UntypedHandle");
        Assert.True(handleProp.TryGetProperty("$type", out var typeVal));
        Assert.Contains("SimpleTestAsset", typeVal.GetString());
        Assert.False(handleProp.TryGetProperty("$path", out _));
    }

    [Fact]
    public void Write_WithUntypedHandle_PathBacked()
    {
        using var world = CreateWorld();
        var server = world.Resources.Get<AssetServer>();
        server.InitAssets<SimpleTestAsset>();

        Handle<SimpleTestAsset> assetHandle = server.GetAssets<SimpleTestAsset>()
            .Add(new SimpleTestAsset { Data = 99 }, new AssetPath("test/my-asset.dat"));

        var entity = world.Spawn(Entity.With(new TestComponentWithUntypedHandle
        {
            UntypedHandle = assetHandle,
            Value = 7,
        }));

        using var writer = new SceneWriter(new() { WriteRoot = true });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var comp = entityData.GetProperty("Components").GetProperty("TestComponentWithUntypedHandle");

        Assert.Equal(7, comp.GetProperty("Value").GetInt32());

        var handleProp = comp.GetProperty("UntypedHandle");
        Assert.True(handleProp.TryGetProperty("$type", out var typeVal));
        Assert.Contains("SimpleTestAsset", typeVal.GetString());
        Assert.True(handleProp.TryGetProperty("$path", out var pathVal));
        Assert.Equal("test/my-asset.dat", pathVal.GetString());
    }

    [Fact]
    public void Write_WithUntypedHandle_Null()
    {
        using var world = CreateWorld();

        var entity = world.Spawn(Entity.With(new TestComponentWithUntypedHandle
        {
            UntypedHandle = Handle.Null,
            Value = 3,
        }));

        using var writer = new SceneWriter(new() { WriteRoot = true });
        var data = writer.Write(world, entity);
        var json = ParseJson(data);

        var root = json.RootElement;
        var entityData = root.GetProperty("Entities")[0];
        var comp = entityData.GetProperty("Components").GetProperty("TestComponentWithUntypedHandle");

        Assert.Equal(3, comp.GetProperty("Value").GetInt32());

        var handleProp = comp.GetProperty("UntypedHandle");
        Assert.False(handleProp.TryGetProperty("$type", out _));
    }
}
