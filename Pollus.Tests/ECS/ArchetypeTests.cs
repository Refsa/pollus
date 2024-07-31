namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class ArchetypeTests
{
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

        var entity = archetype.AddEntity(new(archetype.EntityCount));
        Assert.Equal(1, archetype.EntityCount);
        Assert.Equal(1, archetype.Chunks.Length);
        Assert.Equal(1, archetype.Chunks[0].Count);

        for (int i = 0; i < archetype.GetChunkInfo().RowsPerChunk - 1; i++)
        {
            archetype.AddEntity(new(archetype.EntityCount));
        }

        Assert.Equal(archetype.GetChunkInfo().RowsPerChunk, archetype.Chunks[0].Count);
        Assert.Equal(archetype.GetChunkInfo().RowsPerChunk, archetype.EntityCount);

        var nextEntity = new Entity(archetype.EntityCount);
        archetype.AddEntity(nextEntity);
        Assert.Equal(2, archetype.Chunks.Length);
        Assert.Equal(1, archetype.Chunks[1].Count);

        archetype.RemoveEntity(entity);
        Assert.Equal(archetype.GetChunkInfo().RowsPerChunk, archetype.Chunks[0].Count);
        Assert.Equal(0, archetype.Chunks[1].Count);
        Assert.Equal(nextEntity, archetype.Chunks[0].GetEntities()[0]);

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
            archetype.AddEntity(new(archetype.EntityCount));
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

    [Fact]
    public void ArchetypeStore_CreateEntity()
    {
        using var world = new World();
        var entity = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        for (int i = 0; i < 1_000; i++)
        {
            Entity.With(new TestComponent1 { Value = i }).Spawn(world);
        }
    }

    [Fact]
    public void ArchetypeStore_DestroyEntity()
    {
        using var world = new World();
        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent1 { Value = 20 }).Spawn(world);

        world.Archetypes.DestroyEntity(entity1);

        Assert.False(world.Archetypes.EntityExists(entity1));
        Assert.True(world.Archetypes.EntityExists(entity2));

        var c1 = world.Archetypes.GetComponent<TestComponent1>(entity2);
        Assert.Equal(20, c1.Value);
    }

    [Fact]
    public void ArchetypeStore_AddComponent()
    {
        using var world = new World();
        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);

    }
}