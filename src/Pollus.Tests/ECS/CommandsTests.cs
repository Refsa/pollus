#pragma warning disable CA1416
namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class CommandsTests
{
    [Fact]
    public void Commands_AddComponent_OnExisting()
    {
        using var world = new World();
        var entity = Entity.Null;
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
    public void Commands_AddComponent_Many()
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

    [Fact]
    public void Commands_SetComponent()
    {
        using var world = new World();
        var entity = Entity.Null;

        {
            var commands = world.GetCommands();
            entity = commands.Spawn(Entity.With(new TestComponent1 { Value = 5 })).Entity;
            world.Update();
        }

        {
            var commands = world.GetCommands();
            commands.Entity(entity).SetComponent(new TestComponent1 { Value = 10 });
            world.Update();
        }

        var component1 = world.Store.GetComponent<TestComponent1>(entity);
        Assert.Equal(10, component1.Value);
    }

    [Fact]
    public void Commands_SpawnWithRequiredCompnents()
    {
        using var world = new World();

        var commands = world.GetCommands();
        var entity = commands.Spawn(Entity.With(new TestComponent4 { Value = 5 })).Entity;
        world.Update();

        var component1 = world.Store.GetComponent<TestComponent1>(entity);
        Assert.Equal(111, component1.Value);

        var component4 = world.Store.GetComponent<TestComponent4>(entity);
        Assert.Equal(5, component4.Value);

        var component2 = world.Store.GetComponent<TestComponent2>(entity);
        Assert.Equal(222, component2.Value);
    }

    [Fact]
    public void Commands_Despawn()
    {
        using var world = new World();

        var commands = world.GetCommands();
        var entity = commands.Spawn(Entity.With(new TestComponent1 { Value = 5 })).Entity;
        world.Update();

        Assert.True(world.Store.EntityExists(entity));

        commands = world.GetCommands();
        commands.Despawn(entity);
        world.Update();

        Assert.False(world.Store.EntityExists(entity));
    }

    [Fact]
    public void Commands_Despawn_SameEntityTwice_Throws()
    {
        using var world = new World();

        var commands = world.GetCommands();
        var entity = commands.Spawn(Entity.With(new TestComponent1 { Value = 5 })).Entity;
        world.Update();

        commands = world.GetCommands();
        commands.Despawn(entity);
        commands.Despawn(entity);

        Assert.Throws<ArgumentException>(() => world.Update());
    }

    [Fact]
    public void Commands_Despawn_Many()
    {
        using var world = new World();
        var entities = new List<Entity>();

        var commands = world.GetCommands();
        for (int i = 0; i < 1000; i++)
        {
            entities.Add(commands.Spawn(Entity.With(new TestComponent1 { Value = i })).Entity);
        }

        world.Update();

        commands = world.GetCommands();
        foreach (var entity in entities)
        {
            commands.Despawn(entity);
        }

        world.Update();

        foreach (var entity in entities)
        {
            Assert.False(world.Store.EntityExists(entity));
        }
    }

    [Fact]
    public void Commands_MultipleCommandTypes_NoDuplicateBuffers()
    {
        using var world = new World();

        var commands = world.GetCommands();
        var e1 = commands.Spawn(Entity.With(new TestComponent1 { Value = 1 })).Entity;
        commands.Despawn(e1);
        world.Update();

        commands = world.GetCommands();
        var e2 = commands.Spawn(Entity.With(new TestComponent1 { Value = 2 })).Entity;
        commands.Despawn(e2);
        world.Update();

        Assert.Equal(0, world.Store.EntityCount);
    }

    [Fact]
    public void Commands_BufferCount_StableAcrossFrames()
    {
        using var world = new World();

        var commands = world.GetCommands();
        var entity = commands.Spawn(Entity.With(new TestComponent1 { Value = 1 })).Entity;
        commands.Despawn(entity);
        world.Update();

        commands = world.GetCommands();
        int countBeforeCommands = commands.CommandBufferCount; // should be 2
        commands.Spawn(Entity.With(new TestComponent1 { Value = 2 }));
        commands.Despawn(entity);

        Assert.Equal(countBeforeCommands, commands.CommandBufferCount);
    }
}
#pragma warning restore CA1416