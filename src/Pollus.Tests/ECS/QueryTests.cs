#pragma warning disable CA1416
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
        var q1 = new Query<TestComponent1>(world);
        q1.ForEach<None<TestComponent2>>((ref TestComponent1 c1) =>
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

        var q2 = new Query<TestComponent1>(world);
        index = 1;
        q2.ForEach<All<TestComponent2>>((ref TestComponent1 c1) =>
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
        var query = new Query(world);
        foreach (var row in q)
        {
            row.Component0.Value++;
            count++;
        }

        Assert.Equal(100, count);

        count = 1;
        foreach (var row in q)
        {
            Assert.Equal(count++, row.Component0.Value);
        }
    }

    [Fact]
    public void Query_SetChanged()
    {
        using var world = new World();
        var e1 = Entity.With(new TestComponent1 { Value = 1 }).Spawn(world);
        var e2 = Entity.With(new TestComponent1 { Value = 2 }).Spawn(world);

        var q = new Query(world);
        q.SetChanged<TestComponent1>(e1);
        q.SetChanged<TestComponent1>(e2);

        Assert.True(q.Changed<TestComponent1>(e1));
        Assert.True(q.Changed<TestComponent1>(e2));
    }

    [Fact]
    public void Query_Filter_Added()
    {
        using var world = new World();
        var qTc1 = new Query.Filter<Added<TestComponent1>>(world);
        var qTc2 = new Query.Filter<Added<TestComponent2>>(world);
        var e1 = world.Spawn(Entity.With(new TestComponent1 { Value = 1 }));

        {
            Assert.Equal(1, qTc1.EntityCount());
            Assert.Equal(0, qTc2.EntityCount());

            world.Update();
            Assert.Equal(1, qTc1.EntityCount());
            Assert.Equal(0, qTc2.EntityCount());

            world.Update();
            Assert.Equal(0, qTc1.EntityCount());
            Assert.Equal(0, qTc2.EntityCount());
        }

        world.Store.AddComponent(e1, new TestComponent2 { Value = 2 });
        {
            Assert.Equal(0, qTc1.EntityCount());
            Assert.Equal(1, qTc2.EntityCount());

            world.Update();
            Assert.Equal(0, qTc1.EntityCount());
            Assert.Equal(1, qTc2.EntityCount());

            world.Update();
            Assert.Equal(0, qTc1.EntityCount());
            Assert.Equal(0, qTc2.EntityCount());
        }
    }

    [Fact]
    public void Query_Filter_Changed()
    {
        using var world = new World();
        var qTc1 = new Query.Filter<Changed<TestComponent1>>(world);
        var e1 = world.Spawn(Entity.With(new TestComponent1 { Value = 1 }));
        world.Update();

        world.Store.SetComponent(e1, new TestComponent1 { Value = 2 });
        Assert.Equal(1, qTc1.EntityCount());
        world.Update();
        Assert.Equal(1, qTc1.EntityCount());
        world.Update();
        Assert.Equal(0, qTc1.EntityCount());
    }

    [Fact]
    public void Query_Filter_Removed()
    {
        using var world = new World();
        var qTc1 = new Query.Filter<Removed<TestComponent1>>(world);
        var e1 = world.Spawn(Entity.With(
            new TestComponent1 { Value = 1 },
            new TestComponent2 { Value = 2 }
        ));

        world.Update();
        Assert.Equal(0, qTc1.EntityCount());

        world.Store.RemoveComponent<TestComponent1>(e1);
        Assert.Equal(1, qTc1.EntityCount());
        world.Update();
        Assert.Equal(1, qTc1.EntityCount());
        world.Update();
        Assert.Equal(0, qTc1.EntityCount());
    }
}
#pragma warning restore CA1416