#pragma warning disable CA1416
namespace Pollus.Tests.ECS;

using Pollus.ECS;
using Pollus.Engine.Transform;

public class HierarchyTests
{
    [Fact]
    public void Hierarchy_Commands_AddChild()
    {
        using var world = new World();
        var hierarchies = new List<(Entity parent, Entity child)>();
        var commands = world.GetCommands();

        for (int i = 0; i < 1000; i++)
        {
            var parentEntity = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
            var childEntity = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
            commands.AddChild(parentEntity, childEntity);
            hierarchies.Add((parentEntity, childEntity));
        }

        world.Update();

        foreach (var (parentEntity, childEntity) in hierarchies)
        {
            var parent = world.Store.GetComponent<Parent>(parentEntity);
            var child = world.Store.GetComponent<Child>(childEntity);

            Assert.Equal(childEntity, parent.FirstChild);
            Assert.Equal(parentEntity, child.Parent);
            Assert.Equal(Entity.NULL, child.NextSibling);
            Assert.Equal(Entity.NULL, child.PreviousSibling);
        }
    }

    [Fact]
    public void Hierarchy_Commands_ComplexTree()
    {
        using var world = new World();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var child1 = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var child2 = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var child1_1 = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;

        commands.AddChild(root, child1);
        commands.AddChild(root, child2);
        commands.AddChild(child1, child1_1);

        world.Update();

        var root_parent = world.Store.GetComponent<Parent>(root);
        var child1_parent = world.Store.GetComponent<Parent>(child1);
        var child1_child = world.Store.GetComponent<Child>(child1);
        var child2_child = world.Store.GetComponent<Child>(child2);
        var child1_1_child = world.Store.GetComponent<Child>(child1_1);

        Assert.Equal(root, child1_child.Parent);
        Assert.Equal(root, child2_child.Parent);

        Assert.Equal(child2, root_parent.FirstChild);
        Assert.Equal(child2, child1_child.PreviousSibling);

        Assert.Equal(child1, child2_child.NextSibling);
        Assert.Equal(child1, child1_1_child.Parent);

        Assert.Equal(child1_1, child1_parent.FirstChild);
    }
}
#pragma warning restore CA1416