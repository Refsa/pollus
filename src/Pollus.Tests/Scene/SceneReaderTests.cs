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

[Serialize]
public partial class RootAsset
{
    public required Handle<ChildAsset> Child1;
    public required Handle<ChildAsset> Child2;
}

[Serialize]
public partial class ChildAsset
{
    public required Handle<TextAsset> Text;
}

public partial struct ComplexHandleComponent : IComponent
{
    public required Handle<RootAsset> Root;
}

public class SceneReaderTests
{
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
        var yaml = $@"
types:
  TestComponent: ""{typeof(TestComponent).AssemblyQualifiedName}""
  TestComplexComponent: ""{typeof(TestComplexComponent).AssemblyQualifiedName}""
";

        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(yaml));

        Assert.Equal(2, scene.Types.Count);
        Assert.Equal("TestComponent", scene.Types[0].Name);
        Assert.Equal("TestComplexComponent", scene.Types[1].Name);
    }

    [Fact]
    public void Parse_Entity_Simple()
    {
        using var parser = new SceneReader();
        var yaml = @"
entities:
  Entity1:
    id: 10
";

        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(yaml));

        Assert.Single(scene.Entities);
        Assert.Equal("Entity1", scene.Entities[0].Name);
        Assert.Equal(10, scene.Entities[0].EntityID);
    }

    [Fact]
    public void Parse_Entity_EmptyComponent()
    {
        using var parser = new SceneReader();
        var yaml = $@"
types:
  TestEmptyComponent: ""{typeof(TestEmptyComponent).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      TestEmptyComponent: {{}}
";
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(yaml));

        Assert.Single(scene.Entities);
        Assert.Single(scene.Entities[0].Components);
        var comp = scene.Entities[0].Components[0];
        var empty = MemoryMarshal.AsRef<TestEmptyComponent>(comp.Data);
        Assert.IsType<TestEmptyComponent>(empty);
    }

    [Fact]
    public void Parse_Entity_WithComponent()
    {
        using var parser = new SceneReader();
        var yaml = $@"
types:
  TestComponent: ""{typeof(TestComponent).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      TestComponent:
        Value: 42
";
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(yaml));

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
        var yaml = $@"
types:
  TestComplexComponent: ""{typeof(TestComplexComponent).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      TestComplexComponent:
        Position: {{ X: 10.5, Y: 20.5, Ignore: 42 }}
";
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(yaml));

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
        var yaml = @"
entities:
  Parent:
    children:
      Child1:
      Child2:
        children:
          GrandChild:
";
        var context = CreateContext();
        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(yaml));

        Assert.Single(scene.Entities);
        var parent = scene.Entities[0];
        Assert.Equal("Parent", parent.Name);
        Assert.Equal(2, parent.Children.Length);

        Assert.Equal("Child1", parent.Children[0].Name);
        Assert.Equal("Child2", parent.Children[1].Name);

        Assert.Single(parent.Children[1].Children);
        Assert.Equal("GrandChild", parent.Children[1].Children[0].Name);
    }

    [Fact]
    public void Parse_WithTypedHandle()
    {
        using var parser = new SceneReader();

        var yaml = $@"
types:
  TestComponentWithHandle: ""{typeof(TestComponentWithHandle).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      TestComponentWithHandle:
        Value: 10
        AssetHandle: ""path/to/asset.txt""
";
        var assetIO = new TestAssetIO("assets");
        assetIO.AddFile("path/to/asset.txt", "this is some asset"u8.ToArray());
        var context = CreateContext(assetIO);
        context.AssetServer.AddLoader<TextAssetLoader>();

        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(yaml));

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

        var yaml = $@"
types:
  ComplexHandleComponent: ""{typeof(ComplexHandleComponent).AssemblyQualifiedName}""
  RootAsset: ""{typeof(RootAsset).AssemblyQualifiedName}""
  ChildAsset: ""{typeof(ChildAsset).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      ComplexHandleComponent:
        Root:
          Child1:
            Text: ""path/to/child1.txt""
          Child2:
            Text: ""path/to/child2.txt""
";
        var assetIO = new TestAssetIO("assets")
            .AddFile("path/to/child1.txt", "this is child 1 asset"u8.ToArray())
            .AddFile("path/to/child2.txt", "this is child 2 asset"u8.ToArray());
        var context = CreateContext(assetIO);
        context.AssetServer.AddLoader<TextAssetLoader>();

        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<RootAsset>());
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new HandleSerializer<ChildAsset>());

        var scene = parser.Parse(context, Encoding.UTF8.GetBytes(yaml));

        Assert.Single(scene.Entities);
        Assert.Single(scene.Entities[0].Components);

        var comp = scene.Entities[0].Components[0];
        var value = MemoryMarshal.AsRef<ComplexHandleComponent>(comp.Data);

        Assert.NotEqual(Handle<RootAsset>.Null, value.Root);
    }
}
