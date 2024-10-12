namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.Engine.Physics;
using Pollus.Engine.Transform;

public static class SpatialPlugin
{
    public static SpatialPlugin<SpatialGrid, Empty> Grid(int cellSize, int width, int height)
    {
        return new SpatialPlugin<SpatialGrid, Empty>(new SpatialGrid(cellSize, width, height));
    }

    public static SpatialPlugin<SpatialGrid, TQueryFilters> Grid<TQueryFilters>(int cellSize, int width, int height)
        where TQueryFilters : ITuple, new()
    {
        return new SpatialPlugin<SpatialGrid, TQueryFilters>(new SpatialGrid(cellSize, width, height));
    }
}

public class SpatialPlugin<TSpatialQuery, TQueryFilters> : IPlugin
    where TSpatialQuery : ISpatialQuery
    where TQueryFilters : ITuple, new()
{
    public TSpatialQuery SpatialQuery { get; init; }

    public SpatialPlugin(TSpatialQuery spatialQuery)
    {
        SpatialQuery = spatialQuery;
    }

    public void Apply(World world)
    {
        if (world.Resources.Has<SpatialQuery>()) 
        {
            throw new InvalidOperationException("SpatialQuery already created");
        }

        world.Resources.Add(new SpatialQuery(SpatialQuery));

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create($"SpatialQuery<{typeof(TSpatialQuery).Name}>::Update",
        static (
            SpatialQuery spatialQuery,
            Query<Transform2D, CollisionShape>.Filter<TQueryFilters> qShapes) =>
        {
            spatialQuery.Clear();
            qShapes.ForEach(new UpdateJob() { SpatialQuery = spatialQuery });
        }));
    }

    readonly struct UpdateJob : IEntityForEach<Transform2D, CollisionShape>
    {
        public readonly SpatialQuery SpatialQuery { get; init; }

        public readonly void Execute(in Entity entity, ref Transform2D transform, ref CollisionShape shape)
        {
            var boundingCircle = shape.GetBoundingCircle(transform);
            SpatialQuery.Insert(entity, transform.Position, boundingCircle.Radius, ~0u);
        }
    }
}