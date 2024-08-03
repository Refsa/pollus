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

        var q1 = new Query<TestComponent1>(world);
        q1.ForEach((ref TestComponent1 c1) =>
        {
            c1.Value++;
        });

        int index = 2;
        q1.ForEach((ref TestComponent1 c1) =>
        {
            Assert.Equal(index++, c1.Value);
        });
    }

    [Fact]
    public void Query_IterFilter()
    {
        using var world = new World();
        for (int i = 0; i < 1_000_000; i++)
        {
            Entity.With(new TestComponent1 { Value = i + 1 }).Spawn(world);
            Entity.With(new TestComponent1 { Value = i + 1 }, new TestComponent2 { Value = i + 1 }).Spawn(world);
        }

        var q1 = new Query<TestComponent1>.Filter<Without<TestComponent2>>(world);
        q1.ForEach((ref TestComponent1 c1) =>
        {
            c1.Value++;
        });

        int index = 2;
        q1.ForEach((ref TestComponent1 c1) =>
        {
            Assert.Equal(index++, c1.Value);
        });

        var q2 = new Query<TestComponent1>.Filter<With<TestComponent2>>(world);
        index = 1;
        q2.ForEach((ref TestComponent1 c1) =>
        {
            Assert.Equal(index++, c1.Value);
        });
    }
}