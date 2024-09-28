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
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i, newEntities[i].ID);
            Assert.Equal(1, newEntities[i].Version);
        }
    }
}
