namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class ComponentChangesTests
{
    [Fact]
    public void ComponentChanges_OneFrame_Lifetime()
    {
        using var world = new World();
        var entity = new Entity(1);

        world.Store.Changes.SetFlag(entity, 0, ComponentFlags.Added);
        world.Store.Changes.SetFlag(entity, 1, ComponentFlags.Changed);
        world.Store.Changes.SetFlag(entity, 2, ComponentFlags.Removed);

        world.Update();

        world.Store.Changes.HasFlag(entity, 0, ComponentFlags.Removed);

        Assert.True(world.Store.Changes.HasFlag(entity, 0, ComponentFlags.Added));
        Assert.True(world.Store.Changes.HasFlag(entity, 1, ComponentFlags.Changed));
        Assert.True(world.Store.Changes.HasFlag(entity, 2, ComponentFlags.Removed));

        world.Update();

        Assert.False(world.Store.Changes.HasFlag(entity, 0, ComponentFlags.Added));
        Assert.False(world.Store.Changes.HasFlag(entity, 1, ComponentFlags.Changed));
        Assert.False(world.Store.Changes.HasFlag(entity, 2, ComponentFlags.Removed));
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
        world.Store.Changes.HasFlag<TestComponent1>(entity, ComponentFlags.Removed);
    }
}