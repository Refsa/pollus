namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class ArchetypeTests
{
    /* [Fact]
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
    } */

    [Fact]
    public void NativeMap_Test()
    {
        using var map = new NativeMap<int, int>(0);

        map.Add(0, 10);
        map.Add(1, 20);
        map.Add(2, 30);

        Assert.True(map.Has(0));
        Assert.True(map.Has(1));
        Assert.True(map.Has(2));

        Assert.Equal(10, map.Get(0));
        Assert.Equal(20, map.Get(1));
        Assert.Equal(30, map.Get(2));
    }

    [Fact]
    public void NativeArray_Test()
    {
        using var array = new NativeArray<int>(1_000);

        for (int i = 0; i < 1_000; i++)
        {
            array.Set(i, i);
        }

        for (int i = 0; i < 1_000; i++)
        {
            Assert.Equal(i, array.Get(i));
        }
    }

    [Fact]
    public void ArchetypeChunk_SetGet()
    {
        using var chunk = new ArchetypeChunk([
            Component.GetInfo<TestComponent1>().ID,
        ], 1);

        chunk.SetComponent(0, new TestComponent1 { Value = 10 });
        var tc1 = chunk.GetComponent<TestComponent1>(0);
        Assert.Equal(10, tc1.Value);
    }

    [Fact]
    public void ArchetypeChunk_Few()
    {
        using var chunk = new ArchetypeChunk([
            Component.GetInfo<TestComponent1>().ID,
        ], 1_000);

        for (int i = 0; i < 1_000; i++)
        {
            chunk.SetComponent(i, new TestComponent1 { Value = i });
        }

        for (int i = 0; i < 1_000; i++)
        {
            var tc1 = chunk.GetComponent<TestComponent1>(i);
            Assert.Equal(i, tc1.Value);
        }
    }

    [Fact]
    public void ArchetypeChunk_Many()
    {
        using var chunk = new ArchetypeChunk([
            Component.GetInfo<TestComponent1>().ID,
        ], 1_000_000);

        for (int i = 0; i < 1_000_000; i++)
        {
            chunk.SetComponent(i, new TestComponent1 { Value = i });
        }

        for (int i = 0; i < 1_000_000; i++)
        {
            var tc1 = chunk.GetComponent<TestComponent1>(i);
            Assert.Equal(i, tc1.Value);
        }
    }

    [Fact]
    public void Archetype_AddRemove_Entity()
    {
        Span<ComponentID> cids = [
            Component.GetInfo<TestComponent1>().ID,
            Component.GetInfo<TestComponent2>().ID,
            Component.GetInfo<TestComponent3>().ID,
        ];

        using var archetype = new Archetype(ArchetypeID.Create(cids), cids);

        var entity = archetype.AddEntity();
        Assert.Equal(1, archetype.EntityCount);
        Assert.Equal(1, archetype.Chunks.Length);
        Assert.Equal(1, archetype.Chunks[0].Count);

        for (int i = 0; i < archetype.GetChunkInfo().RowsPerChunk - 1; i++)
        {
            archetype.AddEntity();
        }

        Assert.Equal(archetype.GetChunkInfo().RowsPerChunk, archetype.Chunks[0].Count);
        Assert.Equal(archetype.GetChunkInfo().RowsPerChunk, archetype.EntityCount);

        var nextEntity = archetype.AddEntity();
        Assert.Equal(2, archetype.Chunks.Length);
        Assert.Equal(1, archetype.Chunks[1].Count);

        archetype.RemoveEntity(entity);
        Assert.Equal(archetype.GetChunkInfo().RowsPerChunk, archetype.Chunks[0].Count);
        Assert.Equal(0, archetype.Chunks[1].Count);

        archetype.Optimize();
        Assert.Equal(1, archetype.Chunks.Length);
        Assert.Equal(archetype.GetChunkInfo().RowsPerChunk, archetype.Chunks[0].Count);
    }

    [Fact]
    public void Archetype_ManyEntities()
    {
        Span<ComponentID> cids = [
            Component.GetInfo<TestComponent1>().ID,
            Component.GetInfo<TestComponent2>().ID,
            Component.GetInfo<TestComponent3>().ID,
        ];

        using var archetype = new Archetype(ArchetypeID.Create(cids), cids);

        for (int i = 0; i < 1_000_000; i++)
        {
            archetype.AddEntity();
        }

        Assert.Equal(1_000_000, archetype.EntityCount);
        int sum = 0;
        for (int i = 0; i < archetype.Chunks.Length; i++)
        {
            sum += archetype.Chunks[i].Count;
        }
        Assert.Equal(1_000_000, sum);

        int expectedChunkCount = 1_000_000 / archetype.GetChunkInfo().RowsPerChunk + 1;
        Assert.Equal(expectedChunkCount, archetype.Chunks.Length);
    }
}