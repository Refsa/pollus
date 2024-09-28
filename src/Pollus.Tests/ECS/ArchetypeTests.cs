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
    public void ArchetypeChunk_SwapRemove()
    {
        var chunk1 = new ArchetypeChunk([
            Component.GetInfo<TestComponent1>().ID,
        ], 1_000);

        var chunk2 = new ArchetypeChunk([
            Component.GetInfo<TestComponent1>().ID,
        ], 1_000);

        for (int i = 0; i < 1_000; i++)
        {
            chunk1.AddEntity(new Entity(i + 1));
            chunk1.SetComponent(i, new TestComponent1 { Value = i });
        }
        chunk2.AddEntity(new Entity(1002));

        chunk2.SwapRemoveEntity(0, ref chunk1);

        Assert.Equal(999, chunk1.Count);
        Assert.Equal(1, chunk2.Count);
        Assert.Equal(Entity.NULL, chunk1.entities[999]);

        for (int i = 0; i < 999; i++)
        {
            var tc1 = chunk1.GetComponent<TestComponent1>(i);
            Assert.Equal(i, tc1.Value);
        }

        var tc2 = chunk2.GetComponent<TestComponent1>(0);
        Assert.Equal(999, tc2.Value);

        chunk1.Dispose();
        chunk2.Dispose();
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

        var entityInfo = archetype.AddEntity(new(archetype.EntityCount));
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

        archetype.RemoveEntity(entityInfo.ChunkIndex, entityInfo.RowIndex);
        Assert.Equal(archetype.GetChunkInfo().RowsPerChunk, archetype.Chunks[0].Count);
        Assert.Equal(1, archetype.Chunks.Length);
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
    public void ArchetypeStore_CreateEntity_Many()
    {
        using var world = new World();
        var entity = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        for (int i = 0; i < 1_000; i++)
        {
            Entity.With(new TestComponent1 { Value = i }).Spawn(world);
        }
    }

    [Fact]
    public void ArchetypeStore_CreateEntity_Different()
    {
        using var world = new World();
        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent2 { Value = 20 }).Spawn(world);
        var entity3 = Entity.With(new TestComponent3 { Value = 30 }).Spawn(world);

        var c1 = world.Store.GetComponent<TestComponent1>(entity1);
        var c2 = world.Store.GetComponent<TestComponent2>(entity2);
        var c3 = world.Store.GetComponent<TestComponent3>(entity3);

        Assert.Equal(10, c1.Value);
        Assert.Equal(20, c2.Value);
        Assert.Equal(30, c3.Value);
    }

    [Fact]
    public void ArchetypeStore_DestroyEntity()
    {
        using var world = new World();
        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent1 { Value = 20 }).Spawn(world);

        world.Store.DestroyEntity(entity1);

        Assert.False(world.Store.EntityExists(entity1));
        Assert.True(world.Store.EntityExists(entity2));

        var c1 = world.Store.GetComponent<TestComponent1>(entity2);
        Assert.Equal(20, c1.Value);
    }

    [Fact]
    public void ArchetypeStore_RemoveAllThenAdd()
    {
        using var world = new World();

        var entities = new List<Entity>();
        for (int i = 0; i < 10; i++)
        {
            entities.Add(Entity.With(new TestComponent1 { Value = i }).Spawn(world));
        }

        for (int i = 0; i < 10; i++)
        {
            world.Store.DestroyEntity(entities[i]);
        }

        world.Store.Archetypes[1].Optimize();

        Entity.With(new TestComponent1 { Value = 100 }).Spawn(world);

        Assert.Equal(1, world.Store.Archetypes[1].Chunks.Length);
        Assert.Equal(1, world.Store.EntityCount);
    }

    [Fact]
    public void ArchetypeStore_DestroyEntity_Many_Ascending()
    {
        using var world = new World();

        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent1 { Value = 20 }).Spawn(world);

        for (int i = 0; i < 10000; i++)
        {
            Entity.With(new TestComponent1 { Value = i }).Spawn(world);
        }

        for (int i = 10; i < 10000; i++)
        {
            world.Store.DestroyEntity(new Entity(i));
        }

        Assert.Equal(12, world.Store.EntityCount);
    }

    [Fact]
    public void ArchetypeStore_DestroyEntity_Many_Tons()
    {
        using var world = new World();

        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent1 { Value = 20 }).Spawn(world);

        List<Entity> entities = new();

        for (int k = 0; k < 10; k++)
        {
            for (int i = 0; i < 100_000; i++)
            {
                var entity = Entity.With(new TestComponent1 { Value = i }).Spawn(world);
                entities.Add(entity);
            }
            world.Update();

            for (int i = 0; i < 100_000; i++)
            {
                world.Despawn(entities[i]);
            }
            entities.Clear();
            world.Update();
        }

        Assert.Equal(2, world.Store.EntityCount);
    }

    [Fact]
    public void ArchetypeStore_DestroyEntity_Many_Descending()
    {
        using var world = new World();

        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent1 { Value = 20 }).Spawn(world);

        for (int i = 0; i < 10000; i++)
        {
            Entity.With(new TestComponent1 { Value = i }).Spawn(world);
        }

        for (int i = 10000; i > 10; i--)
        {
            world.Store.DestroyEntity(new Entity(i));
        }

        Assert.Equal(12, world.Store.EntityCount);
    }

    [Fact]
    public void ArchetypeStore_DestroyEntity_Many_RandomOrder()
    {
        using var world = new World();

        var entities = new List<Entity>();
        for (int i = 0; i < 10000; i++)
        {
            entities.Add(Entity.With(new TestComponent1 { Value = i }).Spawn(world));
        }

        while (world.Store.EntityCount > 11)
        {
            var idx = Random.Shared.Next(0, entities.Count);
            var entity = entities.ElementAt(idx);
            entities.Remove(entity);

            world.Store.DestroyEntity(entity);
        }

        Assert.Equal(11, world.Store.EntityCount);
    }

    [Fact]
    public void ArchetypeStore_AddComponent()
    {
        using var world = new World();
        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        world.Store.AddComponent(entity1, new TestComponent2 { Value = 20 });

        var c1 = world.Store.GetComponent<TestComponent1>(entity1);
        var c2 = world.Store.GetComponent<TestComponent2>(entity1);

        Assert.Equal(20, c2.Value);
        Assert.Equal(10, c1.Value);
    }

    [Fact]
    public void ArchetypeStore_AddComponent_EmptyEntity()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        world.Store.AddComponent(entity1, new TestComponent1 { Value = 10 });
        world.Store.AddComponent(entity1, new TestComponent2 { Value = 20 });

        var c1 = world.Store.GetComponent<TestComponent1>(entity1);
        var c2 = world.Store.GetComponent<TestComponent2>(entity1);

        Assert.Equal(10, c1.Value);
        Assert.Equal(20, c2.Value);
    }

    [Fact]
    public void ArchetypeStore_AddComponent_SeqAdd()
    {
        using var world = new World();

        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        world.Store.AddComponent(entity1, new TestComponent2 { Value = 20 });
        var entity2 = Entity.With(new TestComponent1 { Value = 30 }).Spawn(world);
        world.Store.AddComponent(entity2, new TestComponent2 { Value = 40 });
        var entity3 = Entity.With(new TestComponent1 { Value = 50 }).Spawn(world);
        world.Store.AddComponent(entity3, new TestComponent2 { Value = 60 });

        var c1 = world.Store.GetComponent<TestComponent1>(entity1);
        var c2 = world.Store.GetComponent<TestComponent2>(entity1);
        Assert.Equal(10, c1.Value);
        Assert.Equal(20, c2.Value);

        var c3 = world.Store.GetComponent<TestComponent1>(entity2);
        var c4 = world.Store.GetComponent<TestComponent2>(entity2);
        Assert.Equal(30, c3.Value);
        Assert.Equal(40, c4.Value);

        var c5 = world.Store.GetComponent<TestComponent1>(entity3);
        var c6 = world.Store.GetComponent<TestComponent2>(entity3);
        Assert.Equal(50, c5.Value);
        Assert.Equal(60, c6.Value);
    }

    [Fact]
    public void ArchetypeStore_AddComponent_StaggeredAdd()
    {
        using var world = new World();

        var entity1 = Entity.With(new TestComponent1 { Value = 10 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent1 { Value = 30 }).Spawn(world);
        var entity3 = Entity.With(new TestComponent1 { Value = 50 }).Spawn(world);

        world.Store.AddComponent(entity1, new TestComponent2 { Value = 20 });
        world.Store.AddComponent(entity2, new TestComponent2 { Value = 40 });
        world.Store.AddComponent(entity3, new TestComponent2 { Value = 60 });

        var c1 = world.Store.GetComponent<TestComponent1>(entity1);
        var c2 = world.Store.GetComponent<TestComponent2>(entity1);
        Assert.Equal(10, c1.Value);
        Assert.Equal(20, c2.Value);

        var c3 = world.Store.GetComponent<TestComponent1>(entity2);
        var c4 = world.Store.GetComponent<TestComponent2>(entity2);
        Assert.Equal(30, c3.Value);
        Assert.Equal(40, c4.Value);

        var c5 = world.Store.GetComponent<TestComponent1>(entity3);
        var c6 = world.Store.GetComponent<TestComponent2>(entity3);
        Assert.Equal(50, c5.Value);
        Assert.Equal(60, c6.Value);
    }

    [Fact]
    public void ArchetypeStore_RemoveComponent()
    {
        using var world = new World();
        var entity1 = Entity.With(new TestComponent1 { Value = 10 }, new TestComponent2 { Value = 20 }).Spawn(world);
        world.Store.RemoveComponent<TestComponent2>(entity1);

        var c1 = world.Store.GetComponent<TestComponent1>(entity1);
        Assert.Equal(10, c1.Value);

        Assert.Throws<ArgumentException>(() => world.Store.GetComponent<TestComponent2>(entity1));
    }

    [Fact]
    public void ArchetypeStore_RemoveComponent_WithMove()
    {
        using var world = new World();
        var entity1 = Entity.With(new TestComponent1 { Value = 10 }, new TestComponent2 { Value = 20 }).Spawn(world);
        var entity2 = Entity.With(new TestComponent1 { Value = 30 }, new TestComponent2 { Value = 40 }).Spawn(world);

        world.Store.RemoveComponent<TestComponent2>(entity1);

        var e1c1 = world.Store.GetComponent<TestComponent1>(entity1);
        Assert.Equal(10, e1c1.Value);

        var e2c1 = world.Store.GetComponent<TestComponent1>(entity2);
        Assert.Equal(30, e2c1.Value);
        var e2c2 = world.Store.GetComponent<TestComponent2>(entity2);
        Assert.Equal(40, e2c2.Value);

        Assert.Throws<ArgumentException>(() => world.Store.GetComponent<TestComponent2>(entity1));
    }

    [Fact]
    public void ArchetypeStore_Preallocate()
    {
        using var world = new World();
        world.Preallocate(Entity.With(new TestComponent1(), new TestComponent2()), 1_000_000);
        for (int i = 0; i < 1_000_000; i++)
        {
            var entity = world.Spawn(new TestComponent1 { Value = i }, new TestComponent2 { Value = i + 1 });
            var c1 = world.Store.GetComponent<TestComponent1>(entity);
            var c2 = world.Store.GetComponent<TestComponent2>(entity);
            Assert.Equal(i, c1.Value);
            Assert.Equal(i + 1, c2.Value);
        }
    }
}