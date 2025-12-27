namespace Pollus.Tests.Scene;

using Pollus.Tests.Utils;
using Pollus.Engine.Assets;
using Pollus.Engine.Serialization;
using System.Runtime.InteropServices;
using Pollus.Utils;
using Xunit;
using Pollus.Engine;
using Pollus.ECS;
using System.Text;
using Pollus.Core.Serialization;
using System.Runtime.CompilerServices;
using Core.Assets;
using Pollus.Engine.Rendering;
using Pollus.Graphics.Rendering;

public partial struct TestEmptyComponent : IComponent;

public partial struct TestComponent : IComponent
{
    public int Value { get; set; }
}

public partial struct TestComplexComponent : IComponent
{
    public Vec2 Position { get; set; }
}

public partial struct TestComponentWithHandle : IComponent
{
    public int Value { get; set; }
    public Handle<TextAsset> AssetHandle { get; set; }
}

[Serialize]
public partial struct Vec2
{
    public float X { get; set; }
    public float Y { get; set; }
    [SerializeIgnore] public int Ignore { get; set; }
}

[Serialize, Asset]
public partial class RootAsset
{
    public required Handle<ChildAsset> Child1;
    public required Handle<ChildAsset> Child2;
}

[Serialize, Asset]
public partial class ChildAsset
{
    public required Handle<TextAsset> Text;
}

public partial struct ComplexHandleComponent : IComponent
{
    public required Handle<RootAsset> Root;
}

[Serialize]
public partial struct TestHandleObject
{
    public Handle<TextAsset> Image { get; set; }
    public int Value { get; set; }
}

public partial struct TestComponentWithObject : IComponent
{
    public TestHandleObject HandleObject { get; set; }
}

public enum TestEnum
{
    One = 1,
    Two = 2,
    Three = 3,
}

public partial struct TestComponentWithEnum : IComponent
{
    public TestEnum EnumValue { get; set; }
}

public class TestFileTypeMigration : ISceneFileTypeMigration
{
    public int FromVersion => 1;
    public int ToVersion => 2;

    public Type GetType(string typeName, string assemblyQualifiedName)
    {
        if (assemblyQualifiedName == "This.Is.A.Test.Component")
        {
            return typeof(TestComponent);
        }

        return Type.GetType(assemblyQualifiedName) ?? throw new Exception($"Type {assemblyQualifiedName} not found");
    }
}

public partial class SceneReaderTests
{
    partial struct TestInnerComponent : IComponent
    {
        public int Value { get; set; }
    }

    WorldSerializationContext CreateContext(TestAssetIO? assetIO = null)
    {
        return new WorldSerializationContext
        {
            AssetServer = new AssetServer(assetIO ?? new TestAssetIO("assets")),
        };
    }

    [Fact]
    public void Parse_Types()
    {
        using var parser = new SceneReader();
        var json =
            $$"""
              {
                "types": {
                  "TestComponent": "{{typeof(TestComponent).AssemblyQualifiedName}}",
                  "TestComplexComponent": "{{typeof(TestComplexComponent).AssemblyQualifiedName}}",
                  "TestInnerComponent": "{{typeof(TestInnerComponent).AssemblyQualifiedName}}"
                }
              }
              """;

        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Equal(3, scene.Types.Count);
        Assert.True(scene.Types.ContainsKey("TestComponent"));
        Assert.True(scene.Types.ContainsKey("TestComplexComponent"));
        Assert.Equal(typeof(TestComponent), scene.Types["TestComponent"]);
        Assert.Equal(typeof(TestComplexComponent), scene.Types["TestComplexComponent"]);
        Assert.Equal(typeof(TestInnerComponent), scene.Types["TestInnerComponent"]);
    }

    [Fact]
    public void Parse_Entity_Simple()
    {
        using var parser = new SceneReader();
        var json = @"
{
  ""entities"": [
    {
      ""name"": ""Entity1"",
      ""id"": 10
    }
  ]
}";

        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        Assert.Equal("Entity1", scene.Entities[0].Name);
        Assert.Equal(10, scene.Entities[0].EntityID);
    }

    [Fact]
    public void Parse_Entity_EmptyComponent()
    {
        using var parser = new SceneReader();
        var json =
            $$"""
              {
                "types": {
                  "TestEmptyComponent": "{{typeof(TestEmptyComponent).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "id": 10,
                    "name": "Entity1",
                    "components": {
                      "TestEmptyComponent": {}
                    }
                  }
                ]
              }
              """;
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        Assert.NotNull(scene.Entities[0].Components);
        Assert.Single(scene.Entities[0].Components);

        var comp = scene.Entities[0].Components[0];
        var empty = MemoryMarshal.AsRef<TestEmptyComponent>(comp.Data);
        Assert.IsType<TestEmptyComponent>(empty);
    }

    [Fact]
    public void Parse_Entity_WithComponent()
    {
        using var parser = new SceneReader();
        var json =
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
                        "Value": 42
                      }
                    }
                  }
                ]
              }
              """;
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        Assert.Single(scene.Entities[0].Components);

        var comp = scene.Entities[0].Components[0];
        var value = MemoryMarshal.AsRef<TestComponent>(comp.Data);
        Assert.Equal(42, value.Value);
    }

    [Fact]
    public void Parse_Entity_WithInlineObject()
    {
        using var parser = new SceneReader();
        var json =
            $$"""
              {
                "types": {
                  "TestComplexComponent": "{{typeof(TestComplexComponent).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "TestComplexComponent": {
                        "Position": { "X": 10.5, "Y": 20.5, "Ignore": 42 }
                      }
                    }
                  }
                ]
              }
              """;
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        var comp = scene.Entities[0].Components[0];
        var complex = MemoryMarshal.AsRef<TestComplexComponent>(comp.Data);

        Assert.Equal(10.5f, complex.Position.X);
        Assert.Equal(20.5f, complex.Position.Y);
        Assert.Equal(0, complex.Position.Ignore);
    }

    [Fact]
    public void Parse_NestedChildren()
    {
        using var parser = new SceneReader();
        var json = @"
{
  ""entities"": [
    {
      ""name"": ""Parent"",
      ""children"": [
        { ""name"": ""Child1"" },
        {
          ""name"": ""Child2"",
          ""children"": [
            { ""name"": ""GrandChild"" }
          ]
        }
      ]
    }
  ]
}";
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        var parent = scene.Entities[0];
        Assert.Equal("Parent", parent.Name);
        Assert.Equal(2, parent.Children.Count);

        Assert.Equal("Child1", parent.Children[0].Name);
        Assert.Equal("Child2", parent.Children[1].Name);

        Assert.Single(parent.Children[1].Children);
        Assert.Equal("GrandChild", parent.Children[1].Children[0].Name);
    }

    [Fact]
    public void Parse_WithTypedHandle()
    {
        using var parser = new SceneReader();

        var json =
            $$"""
              {
                "types": {
                  "TestComponentWithHandle": "{{typeof(TestComponentWithHandle).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "TestComponentWithHandle": {
                        "Value": 10,
                        "AssetHandle": "path/to/asset.txt"
                      }
                    }
                  }
                ]
              }
              """;
        var assetIO = new TestAssetIO("assets");
        assetIO.AddFile("path/to/asset.txt", "this is some asset"u8.ToArray());
        var context = CreateContext(assetIO);
        context.AssetServer.AddLoader<TextAssetLoader>();

        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        var comp = scene.Entities[0].Components[0];
        var value = MemoryMarshal.AsRef<TestComponentWithHandle>(comp.Data);

        Assert.Equal(10, value.Value);
        Assert.NotEqual(Handle<TextAsset>.Null, value.AssetHandle);
    }

    [Fact]
    public void Parse_WithTypedHandle_Hierarchy()
    {
        using var parser = new SceneReader();

        var json =
            $$"""
              {
                "types": {
                  "ComplexHandleComponent": "{{typeof(ComplexHandleComponent).AssemblyQualifiedName}}",
                  "RootAsset": "{{typeof(RootAsset).AssemblyQualifiedName}}",
                  "ChildAsset": "{{typeof(ChildAsset).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "ComplexHandleComponent": {
                        "Root": {
                          "Child1": { "Text": "path/to/child1.txt" },
                          "Child2": { "Text": "path/to/child2.txt" }
                        }
                      }
                    }
                  }
                ]
              }
              """;
        var assetIO = new TestAssetIO("assets")
            .AddFile("path/to/child1.txt", "this is child 1 asset"u8.ToArray())
            .AddFile("path/to/child2.txt", "this is child 2 asset"u8.ToArray());
        var context = CreateContext(assetIO);
        context.AssetServer.AddLoader<TextAssetLoader>();
        context.AssetServer.InitAssets<TextAsset>();
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<TextAsset>());
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<RootAsset>());
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<ChildAsset>());

        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));
        context.AssetServer.FlushLoading();

        Assert.Single(scene.Entities);
        Assert.Single(scene.Entities[0].Components);

        var comp = scene.Entities[0].Components[0];
        var value = MemoryMarshal.AsRef<ComplexHandleComponent>(comp.Data);

        Assert.NotEqual(Handle<RootAsset>.Null, value.Root);

        var rootAsset = context.AssetServer.GetAssets<RootAsset>().Get(value.Root);
        Assert.NotNull(rootAsset);

        var child1Asset = context.AssetServer.GetAssets<ChildAsset>().Get(rootAsset.Child1);
        Assert.NotNull(child1Asset);
        var child1Text = context.AssetServer.GetAssets<TextAsset>().Get(child1Asset.Text);
        Assert.NotNull(child1Text);
        Assert.Equal("this is child 1 asset", child1Text.Content);

        var child2Asset = context.AssetServer.GetAssets<ChildAsset>().Get(rootAsset.Child2);
        Assert.NotNull(child2Asset);
        var child2Text = context.AssetServer.GetAssets<TextAsset>().Get(child2Asset.Text);
        Assert.NotNull(child2Text);
        Assert.Equal("this is child 2 asset", child2Text.Content);
    }

    [Fact]
    public void Parse_Entity_MultipleComponents_FirstEmpty_Success()
    {
        using var parser = new SceneReader();
        var json =
            $$"""
              {
                "types": {
                  "TestComponent": "{{typeof(TestComponent).AssemblyQualifiedName}}",
                  "TestComplexComponent": "{{typeof(TestComplexComponent).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "TestComponent": {},
                      "TestComplexComponent": {
                        "Position": { "X": 10, "Y": 20 }
                      }
                    }
                  }
                ]
              }
              """;
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        Assert.Equal(2, scene.Entities[0].Components.Count);

        var comp1 = scene.Entities[0].Components[0];
        var val1 = MemoryMarshal.AsRef<TestComponent>(comp1.Data);
        Assert.Equal(0, val1.Value);

        var comp2 = scene.Entities[0].Components[1];
        var val2 = MemoryMarshal.AsRef<TestComplexComponent>(comp2.Data);
        Assert.Equal(10, val2.Position.X);
        Assert.Equal(20, val2.Position.Y);
    }

    [Fact]
    public void Parse_WithObjectSyntax()
    {
        using var parser = new SceneReader();

        var json =
            $$"""
              {
                "types": {
                  "TestComponentWithObject": "{{typeof(TestComponentWithObject).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "TestComponentWithObject": {
                        "HandleObject": { "Image": "path/to/image.txt", "Value": 123 }
                      }
                    }
                  }
                ]
              }
              """;
        var assetIO = new TestAssetIO("assets")
            .AddFile("path/to/image.txt", "image data"u8.ToArray());
        var context = CreateContext(assetIO);
        context.AssetServer.AddLoader<TextAssetLoader>();

        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        var comp = scene.Entities[0].Components[0];
        var bindingComp = MemoryMarshal.AsRef<TestComponentWithObject>(comp.Data);

        Assert.NotEqual(Handle<TextAsset>.Null, bindingComp.HandleObject.Image);
        Assert.Equal(123, bindingComp.HandleObject.Value);
    }

    [Fact]
    public void Parse_WithArraySyntax()
    {
        using var parser = new SceneReader();
        var json =
            $$"""
              {
                "types": {
                  "TestComplexComponent": "{{typeof(TestComplexComponent).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "TestComplexComponent": {
                        "Position": [10, 20]
                      }
                    }
                  }
                ]
              }
              """;
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));
        Assert.Single(scene.Entities);
        var comp = scene.Entities[0].Components[0];
        var complex = MemoryMarshal.AsRef<TestComplexComponent>(comp.Data);
        Assert.Equal(10, complex.Position.X);
        Assert.Equal(20, complex.Position.Y);
    }

    [Fact]
    public void Parse_WithEnum()
    {
        using var parser = new SceneReader();
        var json =
            $$"""
              {
                "types": {
                  "TestComponentWithEnum": "{{typeof(TestComponentWithEnum).AssemblyQualifiedName}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "TestComponentWithEnum": {
                        "EnumValue": "One"
                      }
                    }
                  }
                ]
              }
              """;
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        var comp = scene.Entities[0].Components[0];
        var enumComp = MemoryMarshal.AsRef<TestComponentWithEnum>(comp.Data);
        Assert.Equal(TestEnum.One, enumComp.EnumValue);
    }

    [Fact]
    public void Parse_WithSamplerBindingNearest()
    {
        using var parser = new SceneReader();
        var spriteType = typeof(Sprite).AssemblyQualifiedName!;
        var json =
            $$"""
              {
                "types": {
                  "Sprite": "{{spriteType}}"
                },
                "entities": [
                  {
                    "name": "Entity1",
                    "components": {
                      "Sprite": {
                        "Material": {
                          "ShaderSource": "shaders/builtin/sprite.wgsl",
                          "Texture": { "Image": "sprites/test_sheet.png", "Visibility": "Fragment" },
                          "Sampler": { "Sampler": "nearest", "Visibility": "Fragment" }
                        },
                        "Slice": {
                          "Min": [0, 0],
                          "Max": [1, 1]
                        },
                        "Color": [1, 1, 1, 1]
                      }
                    }
                  }
                ]
              }
              """;
        var assetIO = new TestAssetIO("assets")
            .AddFile("shaders/builtin/sprite.wgsl", "shader"u8.ToArray())
            .AddFile("sprites/test_sheet.png", "img"u8.ToArray());
        var context = CreateContext(assetIO);
        context.AssetServer.AddLoader<TextAssetLoader>();
        context.AssetServer.InitAssets<SpriteMaterial>();
        context.AssetServer.InitAssets<SamplerAsset>();
        context.AssetServer.InitAssets<ShaderAsset>();
        context.AssetServer.InitAssets<Texture2D>();

        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        var spriteComp = scene.Entities[0].Components[0];
        var sprite = MemoryMarshal.AsRef<Sprite>(spriteComp.Data);
        Assert.NotEqual(Handle<SpriteMaterial>.Null, sprite.Material);

        var material = context.AssetServer.GetAssets<SpriteMaterial>().Get(sprite.Material);
        Assert.NotNull(material);
        Assert.NotEqual(Handle<SamplerAsset>.Null, material!.Sampler.Sampler);
    }

    [Fact]
    public void Parse_Entity_WithScene()
    {
        using var parser = new SceneReader();
        var json =
            $$"""
              {
                "entities": [
                    {
                        "name": "Entity1",
                        "scene": "path/to/scene.scene"
                    }
                ]
              }
              """;
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));

        Assert.Single(scene.Entities);
        Assert.Equal("Entity1", scene.Entities[0].Name);
        Assert.NotEqual(Handle<Scene>.Null, scene.Entities[0].Scene);
    }

    [Fact]
    public void Parse_WithFileTypeMigration()
    {
        var json =
            $$"""
              {
                "formatVersion": 1,
                "typesVersion": 1,
                "types": {
                    "TestComponent": "This.Is.A.Test.Component"
                }
              }
              """;

        var context = CreateContext();
        using var parser = new SceneReader(new()
        {
            TypesVersion = 2,
            FileTypeMigrations = [new TestFileTypeMigration()],
        });

        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(json));
        Assert.Single(scene.Types);
        Assert.Equal(typeof(TestComponent), scene.Types["TestComponent"]);
        Assert.Equal(1, scene.FormatVersion);
        Assert.Equal(1, scene.TypesVersion);
    }
}
