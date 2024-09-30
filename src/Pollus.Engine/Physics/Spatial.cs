namespace Pollus.Spatial;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Mathematics;

public class SpatialQuery
{
    KdTree<Entity> tree;

    public SpatialQuery()
    {
        tree = new KdTree<Entity>();
    }

    public void Insert(Entity entity, Vec2f position, float radius, uint layer)
    {
        tree.Insert(entity, position, radius, layer);
    }

    public void Query(Vec2f position, float radius, uint layer, ArrayList<Entity> results)
    {
        tree.RangeSearch(position, radius, layer, results);
    }

    public void Clear()
    {
        tree.Clear();
    }
}