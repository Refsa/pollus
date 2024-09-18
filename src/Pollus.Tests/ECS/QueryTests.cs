using Pollus.ECS;

namespace Pollus.Tests.ECS;

public class QueryTests
{
    [Fact]
    public void Query_IterSimple()
    {
        /* var q1 = new Query<TestComponent1>();
        var q2 = new Query<TestComponent1>.Filter<With<TestComponent1>>();
        var q3 = new Query<TestComponent1>.Filter<(With<TestComponent1>, Without<TestComponent2>)>(); */

        using var world = new World();
        for (int i = 0; i < 1_000_000; i++)
        {
            Entity.With(new TestComponent1 { Value = i + 1 }).Spawn(world);
        }

        int count = 0;
        var q1 = new Query<TestComponent1>(world);
        q1.ForEach((ref TestComponent1 c1) =>
        {
            c1.Value++;
            count++;
        });

        Assert.Equal(1_000_000, count);

        int index = 2;
        q1.ForEach((ref TestComponent1 c1) =>
        {
            Assert.Equal(index++, c1.Value);
        });
    }

    [Fact]
    public void Query_IterFilter_WithWithout()
    {
        using var world = new World();
        for (int i = 0; i < 1_000_000; i++)
        {
            Entity.With(new TestComponent1 { Value = i + 1 }).Spawn(world);
            Entity.With(new TestComponent1 { Value = i + 1 }, new TestComponent2 { Value = i + 1 }).Spawn(world);
        }

        int count = 0;
        var q1 = new Query<TestComponent1>.Filter<None<TestComponent2>>(world);
        q1.ForEach((ref TestComponent1 c1) =>
        {
            c1.Value++;
            count++;
        });

        Assert.Equal(1_000_000, count);

        int index = 2;
        q1.ForEach((ref TestComponent1 c1) =>
        {
            Assert.Equal(index++, c1.Value);
        });

        var q2 = new Query<TestComponent1>.Filter<All<TestComponent2>>(world);
        index = 1;
        q2.ForEach((ref TestComponent1 c1) =>
        {
            Assert.Equal(index++, c1.Value);
        });
    }

    [Fact]
    public void Query_Iter_Foreach()
    {
        using var world = new World();
        var q = new Query<TestComponent1>(world);
        for (int i = 0; i < 100; i++)
        {
            Entity.With(new TestComponent1 { Value = i }).Spawn(world);
        }

        int count = 0;
        foreach (var row in q)
        {
            ref var c1 = ref row.Component0;
            c1.Value++;
            count++;
        }

        Assert.Equal(100, count);

        count = 1;
        foreach (var row in q)
        {
            ref var c1 = ref row.Component0;
            Assert.Equal(count++, c1.Value);
        }
    }
}