namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class ArchetypeTests
{
    [Fact]
    public void archetype_Create()
    {
        var archetype = new Archetype([
            Component.GetInfo<TestComponent1>().ID,
            Component.GetInfo<TestComponent2>().ID,
            Component.GetInfo<TestComponent3>().ID,
        ]);

        Assert.True(archetype.Has<TestComponent1>());
        Assert.True(archetype.Has<TestComponent2>());
        Assert.True(archetype.Has<TestComponent3>());
    }

    [Fact]
    public void archetype_Insert_One()
    {
        var archetype = new Archetype([
            Component.GetInfo<TestComponent1>().ID,
        ]);

        var entity = new Entity(0);
        archetype.Insert(entity);
        archetype.Set(entity, new TestComponent1 { Value = 10 });

        var tc1 = archetype.Get<TestComponent1>(entity);
        Assert.Equal(10, tc1.Value);
    }

    [Fact]
    public void archetype_Insert_Many()
    {
        var archetype = new Archetype([
            Component.GetInfo<TestComponent1>().ID,
        ]);

        for (int i = 0; i < 512; i++)
        {
            var entity = new Entity(i);
            archetype.Insert(entity);
            archetype.Set(entity, new TestComponent1 { Value = i });
        }

        for (int i = 0; i < 512; i++)
        {
            var entity = new Entity(i);
            var tc1 = archetype.Get<TestComponent1>(entity);
            Assert.Equal(i, tc1.Value);
        }
    }

    [Fact]
    public void archetype_Remove()
    {
        var archetype = new Archetype([
            Component.GetInfo<TestComponent1>().ID,
        ]);

        var entity = new Entity(0);
        archetype.Insert(entity);
        archetype.Set(entity, new TestComponent1 { Value = 10 });

        archetype.Remove(entity);
    }
}