namespace Pollus.Benchmark;

using BenchmarkDotNet.Attributes;
using Pollus.Collections;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Spatial;

[MemoryDiagnoser]
// [ReturnValueValidator(failOnError: true)]
public class SpatialBench
{
    struct EntityInsertData
    {
        public Entity entity;
        public Vec2f pos;

        public EntityInsertData(Entity entity, Vec2f pos)
        {
            this.entity = entity;
            this.pos = pos;
        }
    }

    const int ENTITY_COUNT = 32 * 32 * 128;
    SpatialHashGrid<Entity> spatialGrid = new SpatialHashGrid<Entity>(64, 2048 / 64, 2048 / 64);
    SpatialHashGrid<Entity> spatialGridInsert = new(64, 2048 / 64, 2048 / 64);
    EntityInsertData[] entities = new EntityInsertData[ENTITY_COUNT];

    public SpatialBench()
    {
        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                for (int z = 0; z < 128; z++)
                {
                    var entity = new Entity(x + y * 32 + z * 32 * 32);
                    entities[x + y * 32 + z * 32 * 32] = new EntityInsertData(entity, new Vec2f(x, y));
                    spatialGrid.Insert(entity, new Vec2f(x, y), 4, 1u << 0);
                }
    }

    /* [Benchmark]
    public int SpatialGrid_Query()
    {
        Span<Entity> result = stackalloc Entity[1024];
        var count = spatialGrid.Query(new Vec2f(16, 16), 128, 1u << 0, result);
        // Guard.IsTrue(count > 0, "Expected to find at least one entity");
        return count;
    } */

    [Benchmark]
    public SpatialHashGrid<Entity> SpatialGrid_Insert()
    {
        spatialGridInsert.Clear();
        var span = entities.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            spatialGridInsert.Insert(span[i].entity, span[i].pos, 4, 1u << 0);
        }
        return spatialGridInsert;
    }
}