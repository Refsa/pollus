namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class EntityTests
{
    [Fact]
    public void Entities_Create()
    {
        var entities = new Entities();
        var entity = entities.Create();
        Assert.Equal(0, entity.ID);
    }

    [Fact]
    public void Entities_CreateMany()
    {
        var entities = new Entities();
        var entitiesArray = new Entity[10];
        entities.Create(entitiesArray);

        for (int i = 0; i < entitiesArray.Length; i++)
        {
            Assert.Equal(i, entitiesArray[i].ID);
        }
    }

    [Fact]
    public void Entities_CreateMany_AliveCount()
    {
        var entities = new Entities();
        var entitiesArray = new Entity[10];
        entities.Create(entitiesArray);
        Assert.Equal(10, entities.AliveCount);
    }

    [Fact]
    public void Entities_StaleHandle_IsNotAlive()
    {
        var entities = new Entities();
        var original = entities.Create(); // ID=0, Version=0
        entities.Free(original);
        var recycled = entities.Create(); // ID=0, Version=1

        Assert.True(entities.IsAlive(recycled));
        Assert.False(entities.IsAlive(original));
    }

    [Fact]
    public void Entities_Free_IsReused()
    {
        var entities = new Entities();
        var entity = entities.Create();
        entities.Free(entity);

        var newEntity = entities.Create();
        Assert.Equal(entity.ID, newEntity.ID);
        Assert.Equal(entity.Version + 1, newEntity.Version);
    }

    [Fact]
    public void Entities_FreeMany_IsReused()
    {
        var entities = new Entities();
        var entitiesArray = new Entity[10];

        entities.Create(entitiesArray);
        for (int i = 0; i < 10; i++) entities.Free(entitiesArray[i]);

        var newEntities = new Entity[10];
        entities.Create(newEntities);

        var reusedIds = new HashSet<int>(newEntities.Select(e => e.ID));
        Assert.Equal(10, reusedIds.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Contains(i, reusedIds);
            Assert.Equal(1, newEntities[i].Version);
        }
    }

    [Fact]
    public void ReserveId_ReturnsUniqueIds()
    {
        var entities = new Entities();
        var ids = new HashSet<int>();

        for (int i = 0; i < 100; i++)
        {
            var entity = entities.ReserveId();
            Assert.True(ids.Add(entity.ID), $"Duplicate ID: {entity.ID}");
        }

        Assert.Equal(100, ids.Count);
    }

    [Fact]
    public void ReserveId_EntityNotAliveUntilActivate()
    {
        var entities = new Entities();
        var reserved = entities.ReserveId();

        Assert.False(entities.IsAlive(reserved));
        Assert.Equal(0, entities.AliveCount);

        entities.Activate(reserved);

        Assert.True(entities.IsAlive(reserved));
        Assert.Equal(1, entities.AliveCount);
    }

    [Fact]
    public void ReserveId_Plus_Activate_EquivalentToCreate()
    {
        var entitiesA = new Entities();
        var entitiesB = new Entities();

        var created = entitiesA.Create();

        var reserved = entitiesB.ReserveId();
        entitiesB.Activate(reserved);

        Assert.Equal(created.ID, reserved.ID);
        Assert.Equal(created.Version, reserved.Version);
        Assert.True(entitiesA.IsAlive(created));
        Assert.True(entitiesB.IsAlive(reserved));
        Assert.Equal(entitiesA.AliveCount, entitiesB.AliveCount);
    }

    [Fact]
    public void ReserveId_RecycledEntity_GetsIncrementedVersion()
    {
        var entities = new Entities();
        var original = entities.Create(); // ID=0, Version=0
        entities.Free(original);

        var recycled = entities.ReserveId();

        Assert.Equal(original.ID, recycled.ID);
        Assert.Equal(original.Version + 1, recycled.Version);
    }
}
