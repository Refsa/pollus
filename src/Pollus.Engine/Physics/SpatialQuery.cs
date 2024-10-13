namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.Mathematics;

public class SpatialQuery : ISpatialContainer<Entity>
{
    readonly ISpatialContainer<Entity> inner;

    public SpatialQuery(ISpatialContainer<Entity> query)
    {
        this.inner = query;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Clear()
    {
        inner.Clear();
    }

    public void Prepare()
    {
        inner.Prepare();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert(Entity entity, Vec2f position, float radius, uint layer)
    {
        inner.Insert(entity, position, radius, layer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Query(Vec2f position, float radius, uint layer, Span<Entity> results)
    {
        return inner.Query(position, radius, layer, results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Insert<TLayer>(Entity entity, Vec2f position, float radius, TLayer layer) where TLayer : unmanaged, Enum
    {
        inner.Insert(entity, position, radius, layer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Query<TLayer>(Vec2f position, float radius, TLayer layer, Span<Entity> results) where TLayer : unmanaged, Enum
    {
        return inner.Query(position, radius, layer, results);
    }
}