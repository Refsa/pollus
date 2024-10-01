namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
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

    public void Insert<TLayer>(Entity entity, Vec2f position, float radius, TLayer layer)
        where TLayer : unmanaged, Enum
    {
        cache.Insert(entity, position, radius, Unsafe.As<TLayer, uint>(ref layer));
    }

    public void Query(Vec2f position, float radius, uint layer, ArrayList<Entity> results)
    {
        cache.Query(position, radius, layer, results);
    }

    public void Query<TLayer>(Vec2f position, float radius, TLayer layer, ArrayList<Entity> results)
        where TLayer : unmanaged, Enum
    {
        cache.Query(position, radius, Unsafe.As<TLayer, uint>(ref layer), results);
    }

    public void Clear()
    {
        cache.Clear();
    }
}