using Pollus.ECS;

namespace Pollus.Tests.ECS;

public class CommandsTests
{
    [Fact]
    public void Commands_AddComponent_OnExisting()
    {
        using var world = new World();
        var entity = Entity.NULL;
        {
            var commands = world.GetCommands();
            entity = commands.Spawn(Entity.With(new TestComponent1 { Value = 5 })).Entity;
            world.Update();
        }

        {
            var commands = world.GetCommands();
            commands.AddComponent(entity, new TestComponent2 { Value = 10 });
            world.Update();
        }

        var component1 = world.Store.GetComponent<TestComponent1>(entity);
        var component2 = world.Store.GetComponent<TestComponent2>(entity);

        Assert.Equal(5, component1.Value);
        Assert.Equal(10, component2.Value);
    }

    [Fact]
    public void Commands_AddComponent_OnExisting_Many()
    {
        using var world = new World();
        var entities = new List<Entity>();

        var commands = world.GetCommands();
        for (int i = 0; i < 1000; i++)
        {
            entities.Add(commands.Spawn(Entity.With(new TestComponent1 { Value = 5 + i * 1000 })).Entity);
        }
        world.Update();

        commands = world.GetCommands();
        for (int i = 0; i < 1000; i++)
        {
            commands.AddComponent(entities[i], new TestComponent2 { Value = 10 + i * 1000 });
        }
        world.Update();

        var index = 0;
        foreach (var entity in entities)
        {
            var component1 = world.Store.GetComponent<TestComponent1>(entity);
            var component2 = world.Store.GetComponent<TestComponent2>(entity);

            Assert.Equal(5 + index * 1000, component1.Value);
            Assert.Equal(10 + index * 1000, component2.Value);
            index++;
        }
    }

    [Fact]
    public void Commands_AddComponent_OnSpawn()
    {
        using var world = new World();
        var entities = new List<Entity>();

        var commands = world.GetCommands();
        for (int i = 0; i < 1000; i++)
        {
            var entity = commands.Spawn(Entity.With(new TestComponent1 { Value = 5 + i * 1000 })).Entity;
            commands.AddComponent(entity, new TestComponent2 { Value = 10 + i * 1000 });
            entities.Add(entity);
        }
        world.Update();

        var index = 0;
        foreach (var entity in entities)
        {
            var component1 = world.Store.GetComponent<TestComponent1>(entity);
            var component2 = world.Store.GetComponent<TestComponent2>(entity);

            Assert.Equal(5 + index * 1000, component1.Value);
            Assert.Equal(10 + index * 1000, component2.Value);
            index++;
        }
    }

    [Fact]
    public void Commands_AddComponent_wearfhjfhjuiweauiwoaefhweafi()
    {
        using var world = new World();

        var hierarchies = new List<(Entity parent, Entity child)>();
        var commands = world.GetCommands();

        for (int i = 0; i < 400; i++)
        {
            var parentEntity = commands.Spawn(Entity.With(new TestComponent1 { Value = i })).Entity;
            var childEntity = commands.Spawn(Entity.With(new TestComponent1 { Value = i })).Entity;
            commands.AddComponent(parentEntity, new TestComponent2 { Value = i * 10 });
            commands.AddComponent(childEntity, new TestComponent2 { Value = i * 100 });
            hierarchies.Add((parentEntity, childEntity));
        }

        world.Update();

        var index = 0;
        foreach (var (parentEntity, childEntity) in hierarchies)
        {
            ref var parentC1 = ref world.Store.GetComponent<TestComponent1>(parentEntity);
            ref var parentC2 = ref world.Store.GetComponent<TestComponent2>(parentEntity);
            ref var childC1 = ref world.Store.GetComponent<TestComponent1>(childEntity);
            ref var childC2 = ref world.Store.GetComponent<TestComponent2>(childEntity);

            Assert.Equal(index, parentC1.Value);
            Assert.Equal(index * 10, parentC2.Value);
            Assert.Equal(index, childC1.Value);
            Assert.Equal(index * 100, childC2.Value);
            index++;
        }
    }
}