namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class ComponentChangesTests
{
    [Fact]
    public void ComponentChanges_OneFrame_Lifetime()
    {
        using var world = new World();
        var entity = new Entity(100);
        for (int i = 0; i < 10; i++) world.Update();

        world.Store.Changes.SetRemoved(entity, 0);

        world.Update();
        Assert.True(world.Store.Changes.WasRemoved(entity, 0));

        world.Update();
        Assert.False(world.Store.Changes.WasRemoved(entity, 0));
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
        Assert.True(world.Store.Changes.WasRemoved<TestComponent1>(entity));
    }
}