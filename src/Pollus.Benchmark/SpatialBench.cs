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
    SpatialHashGrid<Entity> spatialHashGrid = new(64, 2048 / 64, 2048 / 64);
    SpatialHashGrid<Entity> spatialHashGridInsert = new(64, 2048 / 64, 2048 / 64);

    SpatialLooseGrid<Entity> spatialLooseGrid = new(64, 256, 512);
    SpatialLooseGrid<Entity> spatialLooseGridInsert = new(64, 256, 512);

    EntityInsertData[] entities = new EntityInsertData[ENTITY_COUNT];

    public SpatialBench()
    {
        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                for (int z = 0; z < 128; z++)
                {
                    var entity = new Entity(x + y * 32 + z * 32 * 32);
                    entities[x + y * 32 + z * 32 * 32] = new EntityInsertData(entity, new Vec2f(x * 64, y * 64));
                    spatialHashGrid.Insert(entity, new Vec2f(x * 64, y * 64), 4, 1u << 0);
                    spatialLooseGrid.Insert(entity, new Vec2f(x * 64, y * 64), 4, 1u << 0);
                }
    }

    [Benchmark]
    public int SpatialHashGrid_Query()
    {
        Span<Entity> result = stackalloc Entity[1024];
        var count = spatialHashGrid.Query(new Vec2f(0, 0), 128f, 1u << 0, result);
        // Guard.IsTrue(count > 0, "Expected to find at least one entity");
        return count;
    }

    [Benchmark]
    public int SpatialLooseGrid_Query()
    {
        Span<Entity> result = stackalloc Entity[1024];
        var count = spatialLooseGrid.Query(new Vec2f(0, 0), 128f, 1u << 0, result);
        return count;
    }

    /* [Benchmark]
    public SpatialHashGrid<Entity> SpatialHashGrid_Insert()
    {
        spatialHashGridInsert.Clear();
        var span = entities.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            spatialHashGridInsert.Insert(span[i].entity, span[i].pos, 4, 1u << 0);
        }
        return spatialHashGridInsert;
    } */

    /* [Benchmark]
    public SpatialLooseGrid<Entity> SpatialLooseGrid_Insert()
    {
        spatialLooseGridInsert.Clear();
        var span = entities.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            spatialLooseGridInsert.Insert(span[i].entity, span[i].pos, 4, 1u << 0);
        }
        return spatialLooseGridInsert;
    } */
}