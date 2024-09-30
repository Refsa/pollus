namespace Pollus.Spatial;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Mathematics;

public class SpatialQuery
{
    SpatialHashGrid<Entity> cache;

    public SpatialQuery(int cellSize, int width, int height)
    {
        cache = new SpatialHashGrid<Entity>(cellSize, width, height);
    }

    public void Insert(Entity entity, Vec2f position, float radius, uint layer)
    {
        cache.Insert(entity, position, radius, layer);
    }

    public void Query(Vec2f position, float radius, uint layer, ArrayList<Entity> results)
    {
        cache.Query(position, radius, layer, results);
    }

    public void Clear()
    {
        cache.Clear();
    }
}