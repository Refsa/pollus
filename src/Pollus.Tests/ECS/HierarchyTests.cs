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
            var parentEntity = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform()));
            var childEntity = commands.Spawn(Entity.With(new Transform2D(), new GlobalTransform()));
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
}
