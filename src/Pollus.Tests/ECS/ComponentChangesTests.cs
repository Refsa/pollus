namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class ComponentChangesTests
{
    [Fact]
    public void ChangeTracker_OneFrame_Lifetime()
    {
        using var world = new World();
        var entity = new Entity(100);
        for (int i = 0; i < 10; i++) world.Update();

        world.Store.Removed.GetTracker<TestComponent1>().SetRemoved(entity, new TestComponent1 { Value = 321 });
        Assert.True(world.Store.Removed.GetTracker<TestComponent1>().WasRemoved(entity));
        Assert.Equal(321, world.Store.Removed.GetTracker<TestComponent1>().GetRemoved(entity).Value);

        world.Update();
        Assert.True(world.Store.Removed.GetTracker<TestComponent1>().WasRemoved(entity));
        Assert.Equal(321, world.Store.Removed.GetTracker<TestComponent1>().GetRemoved(entity).Value);

        world.Update();
        Assert.False(world.Store.Removed.GetTracker<TestComponent1>().WasRemoved(entity));
        Assert.Throws<NullReferenceException>(() => world.Store.Removed.GetTracker<TestComponent1>().GetRemoved(entity));
    }

    [Fact]
    public void ArchetypeChunk_Flags_OneFrame_LifeTime()
    {
        using var world = new World();
        var entity = world.Spawn(Entity.With(new TestComponent1 { Value = 1 }));
        var info = world.Store.GetEntityInfo(entity);

        ref var chunk = ref world.Store.Archetypes[info.ArchetypeIndex].Chunks[info.ChunkIndex];
        Assert.True(chunk.HasFlag<TestComponent1>(info.RowIndex, ComponentFlags.Added));

        world.Update();
        Assert.True(chunk.HasFlag<TestComponent1>(info.RowIndex, ComponentFlags.Added));

        world.Update();
        Assert.False(chunk.HasFlag<TestComponent1>(info.RowIndex, ComponentFlags.Added));
    }

    [Fact]
    public void AddComponent_Sets_Flag()
    {
        using var world = new World();
        var entity = world.Spawn(Entity.With(new TestComponent1 { Value = 1 }));
        var info = world.Store.GetEntityInfo(entity);

        ref var chunk = ref world.Store.Archetypes[info.ArchetypeIndex].Chunks[info.ChunkIndex];
        Assert.True(chunk.HasFlag<TestComponent1>(info.RowIndex, ComponentFlags.Added));
    }

    [Fact]
    public void RemoveComponent_Sets_Flag()
    {
        using var world = new World();
        var entity = world.Spawn(Entity.With(new TestComponent1 { Value = 1 }));
        world.Store.RemoveComponent<TestComponent1>(entity);

        var info = world.Store.GetEntityInfo(entity);
        Assert.True(world.Store.Removed.GetTracker<TestComponent1>().WasRemoved(entity));
    }

    [Fact]
    public void SpawnAndCloneAfterRemovalOnlyFlagPresentComponents()
    {
        using var world = new World();
        var original = world.Spawn(Entity.With(new TestComponent1 { Value = 1 }, new TestComponent2()));
        world.Store.RemoveComponent<TestComponent2>(original);
        var originalInfo = world.Store.GetEntityInfo(original);
        Assert.True(world.Store.Archetypes[originalInfo.ArchetypeIndex].Chunks[originalInfo.ChunkIndex].HasFlag<TestComponent2>(-1, ComponentFlags.Removed));
        world.Update();
        world.Update();
        var commands = world.GetCommands();
        var spawned = commands.Spawn(Entity.With(new TestComponent1 { Value = 2 })).Entity;
        var cloned = commands.Clone(original).Entity;

        world.Update();

        Assert.Equal(2, world.Store.GetComponent<TestComponent1>(spawned).Value);
        Assert.Equal(1, world.Store.GetComponent<TestComponent1>(cloned).Value);
        foreach (var entity in new[] { spawned, cloned })
        {
            var info = world.Store.GetEntityInfo(entity);
            Assert.Equal(originalInfo.ArchetypeIndex, info.ArchetypeIndex);
            ref var chunk = ref world.Store.Archetypes[info.ArchetypeIndex].Chunks[info.ChunkIndex];
            Assert.True(chunk.HasFlag<TestComponent1>(info.RowIndex, ComponentFlags.Added));
            Assert.False(chunk.HasComponent<TestComponent2>());
            Assert.False(chunk.HasFlag<TestComponent2>(-1, ComponentFlags.Added));
            Assert.False(chunk.HasFlag<TestComponent2>(-1, ComponentFlags.Removed));
        }
    }
}