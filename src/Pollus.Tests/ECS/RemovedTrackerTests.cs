namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class RemovedTrackerTests
{
    [Fact]
    public void RemovedTracker_SetRemoved_UpdatePath_DoesNotPersist()
    {
        using var world = new World();
        var entity = Entity.With(new TestComponent1 { Value = 100 }).Spawn(world);

        world.Store.RemoveComponent<TestComponent1>(entity);

        world.Store.AddComponent(entity, new TestComponent1 { Value = 200 });

        world.Store.RemoveComponent<TestComponent1>(entity);

        ref var removed = ref world.Store.Removed.GetRemoved<TestComponent1>(entity);
        Assert.Equal(200, removed.Value);
    }

    [Fact]
    public void RemovedTracker_Tick_SkipsSwappedEntries()
    {
        using var world = new World();

        var entityA = Entity.With(new TestComponent1 { Value = 1 }).Spawn(world);
        var entityB = Entity.With(new TestComponent1 { Value = 2 }).Spawn(world);
        var entityC = Entity.With(new TestComponent1 { Value = 3 }).Spawn(world);

        world.Store.RemoveComponent<TestComponent1>(entityA);
        world.Store.RemoveComponent<TestComponent1>(entityB);
        world.Store.RemoveComponent<TestComponent1>(entityC);

        world.Store.AddComponent(entityB, new TestComponent1 { Value = 20 });

        world.Store.Tick(2);

        var tracker = world.Store.Removed.GetTracker<TestComponent1>();
        Assert.False(tracker.WasRemoved(entityA));
        Assert.False(tracker.WasRemoved(entityB));
        Assert.False(tracker.WasRemoved(entityC));
    }
}