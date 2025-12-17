namespace Pollus.Tests.Scene;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Utils;
using Xunit;
using Pollus.Engine;
using Pollus.ECS;
using System.Text;
using Pollus.Core.Serialization;

[Serialize]
public partial struct TestEmptyComponent : IComponent;

[Serialize]
public partial struct TestComponent : IComponent
{
    public int Value { get; set; }
}

[Serialize]
public partial struct TestComplexComponent : IComponent
{
    public Vec2 Position { get; set; }
}

[Serialize]
public partial struct TestComponentWithHandle : IComponent
{
    public int Value { get; set; }
    public Handle<TestAsset> AssetHandle { get; set; }
}

[Serialize]
public partial struct Vec2
{
    public float X { get; set; }
    public float Y { get; set; }
    [SerializeIgnore] public int Ignore { get; set; }
}

public class TestAsset;

public class SceneParserTests
{
    static SceneParserTests()
    {
        RuntimeHelpers.RunClassConstructor(typeof(Handle<TestAsset>).TypeHandle);
    }

    [Fact]
    public void Parse_Types()
    {
        var parser = new SceneParser();
        var yaml = $@"
types:
  TestComponent: ""{typeof(TestComponent).AssemblyQualifiedName}""
  TestComplexComponent: ""{typeof(TestComplexComponent).AssemblyQualifiedName}""
";
        var scene = parser.Parse(Encoding.UTF8.GetBytes(yaml));

        Assert.Equal(2, scene.Types.Length);
        Assert.Equal("TestComponent", scene.Types[0].Name);
        Assert.Equal("TestComplexComponent", scene.Types[1].Name);
    }

    [Fact]
    public void Parse_Entity_Simple()
    {
        var parser = new SceneParser();
        var yaml = @"
entities:
  Entity1:
    id: 10
";
        var scene = parser.Parse(Encoding.UTF8.GetBytes(yaml));

        Assert.Single(scene.Entities);
        Assert.Equal("Entity1", scene.Entities[0].Name);
        Assert.Equal(10, scene.Entities[0].EntityID);
    }

    [Fact]
    public void Parse_Entity_EmptyComponent()
    {
        var parser = new SceneParser();
        var yaml = $@"
types:
  TestEmptyComponent: ""{typeof(TestEmptyComponent).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      TestEmptyComponent: {{}}
";
        var scene = parser.Parse(Encoding.UTF8.GetBytes(yaml));

        Assert.Single(scene.Entities);
        Assert.Single(scene.Entities[0].Components);
        var comp = scene.Entities[0].Components[0];
        var empty = MemoryMarshal.AsRef<TestEmptyComponent>(comp.Data);
        Assert.IsType<TestEmptyComponent>(empty);
    }

    [Fact]
    public void Parse_Entity_WithComponent()
    {
        var parser = new SceneParser();
        var yaml = $@"
types:
  TestComponent: ""{typeof(TestComponent).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      TestComponent:
        Value: 42
";
        var scene = parser.Parse(Encoding.UTF8.GetBytes(yaml));

        Assert.Single(scene.Entities);
        Assert.Single(scene.Entities[0].Components);

        var comp = scene.Entities[0].Components[0];
        var value = MemoryMarshal.AsRef<TestComponent>(comp.Data);
        Assert.Equal(42, value.Value);
    }

    [Fact]
    public void Parse_Entity_WithInlineObject()
    {
        var parser = new SceneParser();
        var yaml = $@"
types:
  TestComplexComponent: ""{typeof(TestComplexComponent).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      TestComplexComponent:
        Position: {{ X: 10.5, Y: 20.5, Ignore: 42 }}
        Name: ""Test""
";
        var scene = parser.Parse(Encoding.UTF8.GetBytes(yaml));

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
        var parser = new SceneParser();
        var yaml = @"
entities:
  Parent:
    children:
      Child1:
      Child2:
        children:
          GrandChild:
";
        var scene = parser.Parse(Encoding.UTF8.GetBytes(yaml));

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
    public void Parse_WithHandle()
    {
        var parser = new SceneParser();

        var yaml = $@"
types:
  TestComponentWithHandle: ""{typeof(TestComponentWithHandle).AssemblyQualifiedName}""
entities:
  Entity1:
    components:
      TestComponentWithHandle:
        Value: 10
        AssetHandle: ""path/to/asset""
";
        var scene = parser.Parse(Encoding.UTF8.GetBytes(yaml));

        Assert.Single(scene.Entities);
        var comp = scene.Entities[0].Components[0];
        var value = MemoryMarshal.AsRef<TestComponentWithHandle>(comp.Data);

        Assert.Equal(10, value.Value);
        Assert.Equal(123, value.AssetHandle.ID);
    }
}
