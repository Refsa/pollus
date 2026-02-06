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
            Assert.Equal(Entity.Null, child.NextSibling);
            Assert.Equal(Entity.Null, child.PreviousSibling);
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

        Assert.Equal(child1, root_parent.FirstChild);
        Assert.Equal(child2, child1_child.NextSibling);
        Assert.Equal(Entity.Null, child1_child.PreviousSibling);

        Assert.Equal(child2, root_parent.LastChild);
        Assert.Equal(child1, child2_child.PreviousSibling);
        Assert.Equal(Entity.Null, child2_child.NextSibling);

        Assert.Equal(child1, child1_1_child.Parent);
        Assert.Equal(child1_1, child1_parent.FirstChild);
        Assert.Equal(child1_1, child1_parent.LastChild);
        Assert.Equal(Entity.Null, child1_1_child.NextSibling);
        Assert.Equal(Entity.Null, child1_1_child.PreviousSibling);
    }

    [Fact]
    public void Hierarchy_Commands_MultipleChildren()
    {
        using var world = new World();
        var commands = world.GetCommands();

        var child1 = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var child2 = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var child3 = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var child4 = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var root = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).AddChildren([
            child1, child2, child3, child4
        ]).Entity;

        world.Update();

        var root_parent = world.Store.GetComponent<Parent>(root);
        Assert.Equal(child1, root_parent.FirstChild);
        Assert.Equal(child4, root_parent.LastChild);

        var child1_child = world.Store.GetComponent<Child>(child1);
        Assert.Equal(child2, child1_child.NextSibling);
        Assert.Equal(Entity.Null, child1_child.PreviousSibling);

        var child2_child = world.Store.GetComponent<Child>(child2);
        Assert.Equal(child3, child2_child.NextSibling);
        Assert.Equal(child1, child2_child.PreviousSibling);

        var child3_child = world.Store.GetComponent<Child>(child3);
        Assert.Equal(child4, child3_child.NextSibling);
        Assert.Equal(child2, child3_child.PreviousSibling);

        var child4_child = world.Store.GetComponent<Child>(child4);
        Assert.Equal(Entity.Null, child4_child.NextSibling);
        Assert.Equal(child3, child4_child.PreviousSibling);
    }

    [Fact]
    public void Hierarchy_ChildRemoved_When_ParentEntityDestroyed()
    {
        using var world = new World();
        world.AddPlugin(new HierarchyPlugin());
        world.Prepare();

        var commands = world.GetCommands();
        var parent = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var child = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        commands.AddChild(parent, child);
        world.Update();
        commands = world.GetCommands();
        commands.Despawn(parent);
        world.Update();

        Assert.False(world.Store.HasComponent<Child>(child));
    }

    [Fact]
    public void Hierarchy_ParentChildCountDecremented_When_ChildDestroyed()
    {
        using var world = new World();
        world.AddPlugin(new HierarchyPlugin());
        world.Prepare();

        var commands = world.GetCommands();
        var parent = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        var child = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform())).Entity;
        commands.AddChild(parent, child);
        world.Update();

        ref var cParent = ref world.Store.GetComponent<Parent>(parent);
        Assert.Equal(1, cParent.ChildCount);

        commands = world.GetCommands();
        commands.Despawn(child);
        world.Update();

        Assert.Equal(0, cParent.ChildCount);
    }

    [Fact]
    public void RemoveChildrenCommand_RemovesAllChildren()
    {
        using var world = new World();
        var commands = world.GetCommands();

        var parent = commands.Spawn(Entity.With(new TestComponent1())).Entity;
        var child1 = commands.Spawn(Entity.With(new TestComponent1())).Entity;
        var child2 = commands.Spawn(Entity.With(new TestComponent1())).Entity;
        var child3 = commands.Spawn(Entity.With(new TestComponent1())).Entity;

        commands.AddChild(parent, child1);
        commands.AddChild(parent, child2);
        commands.AddChild(parent, child3);
        world.Update();

        commands = world.GetCommands();
        commands.RemoveChildren(parent);
        world.Update();

        Assert.False(world.Store.HasComponent<Parent>(parent));

        Assert.False(world.Store.HasComponent<Child>(child1));
        Assert.False(world.Store.HasComponent<Child>(child2));
        Assert.False(world.Store.HasComponent<Child>(child3));

        Assert.True(world.Store.EntityExists(child1));
        Assert.True(world.Store.EntityExists(child2));
        Assert.True(world.Store.EntityExists(child3));
    }
}
#pragma warning restore CA1416