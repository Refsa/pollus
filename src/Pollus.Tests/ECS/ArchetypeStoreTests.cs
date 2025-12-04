namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class ArchetypeStoreTests
{
    [Fact]
    public void ArchetypeStore_SetComponent()
    {
        using var world = new World();
        var entity = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);

        world.Store.SetComponent(entity, new TestComponent1 { Value = 20 });
        var c1 = world.Store.GetComponent<TestComponent1>(entity);
        Assert.Equal(20, c1.Value);
    }

    [Fact]
    public void ArchetypeStore_AddComponent()
    {
        using var world = new World();
        var entity = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);

        world.Store.AddComponent(entity, new TestComponent2 { Value = 20 });

        var c1 = world.Store.GetComponent<TestComponent1>(entity);
        var c2 = world.Store.GetComponent<TestComponent2>(entity);

        Assert.Equal(10, c1.Value);
        Assert.Equal(20, c2.Value);
    }

    [Fact]
    public void ArchetypeStore_RemoveComponent()
    {
        using var world = new World();
        var entity = Entity.With(new TestComponent1 { Value = 10 }).With(new TestComponent2 { Value = 20 }).Spawn(world);

        world.Store.RemoveComponent<TestComponent2>(entity);

        var c1 = world.Store.GetComponent<TestComponent1>(entity);
        Assert.Equal(10, c1.Value);
        Assert.False(world.Store.HasComponent<TestComponent2>(entity));
    }

    [Fact]
    public void ArchetypeStore_RemoveComponent_WithSwap()
    {
        using var world = new World();

        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).With(new TestComponent2 { Value = 20 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent1 { Value = 30 }).With(new TestComponent2 { Value = 40 }).Spawn(world);

        world.Store.RemoveComponent<TestComponent2>(entity1);

        var e1c1 = world.Store.GetComponent<TestComponent1>(entity1);
        Assert.Equal(10, e1c1.Value);
        Assert.False(world.Store.HasComponent<TestComponent2>(entity1));

        Assert.True(world.Store.EntityExists(entity2), "Entity2 should still exist");

        var e2c1 = world.Store.GetComponent<TestComponent1>(entity2);
        Assert.Equal(30, e2c1.Value);

        var e2c2 = world.Store.GetComponent<TestComponent2>(entity2);
        Assert.Equal(40, e2c2.Value);
    }

    [Fact]
    public void ArchetypeStore_RemoveComponent_ManyEntities_VerifyIntegrity()
    {
        using var world = new World();
        int count = 100;
        var entities = new Entity[count];

        for (int i = 0; i < count; i++)
        {
            entities[i] = Entity.With(new TestComponent1 { Value = i }).With(new TestComponent2 { Value = i * 2 }).Spawn(world);
        }

        for (int i = 0; i < count; i += 2)
        {
            world.Store.RemoveComponent<TestComponent2>(entities[i]);
        }

        for (int i = 0; i < count; i++)
        {
            var c1 = world.Store.GetComponent<TestComponent1>(entities[i]);
            Assert.Equal(i, c1.Value);

            if (i % 2 == 0)
            {
                Assert.False(world.Store.HasComponent<TestComponent2>(entities[i]));
            }
            else
            {
                Assert.True(world.Store.HasComponent<TestComponent2>(entities[i]));
                var c2 = world.Store.GetComponent<TestComponent2>(entities[i]);
                Assert.Equal(i * 2, c2.Value);
            }
        }
    }
}
