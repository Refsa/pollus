namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.Engine.Physics;
using Pollus.Engine.Transform;

public static class SpatialPlugin
{
    public static SpatialPlugin<SpatialHashGrid<Entity>, NoFilter> Grid(int cellSize, int width, int height)
    {
        return new SpatialPlugin<SpatialHashGrid<Entity>, NoFilter>(new SpatialHashGrid<Entity>(cellSize, width, height));
    }

    public static SpatialPlugin<SpatialHashGrid<Entity>, TQueryFilters> Grid<TQueryFilters>(int cellSize, int width, int height)
        where TQueryFilters : ITuple, IFilter, new()
    {
        return new SpatialPlugin<SpatialHashGrid<Entity>, TQueryFilters>(new SpatialHashGrid<Entity>(cellSize, width, height));
    }

    public static SpatialPlugin<SpatialLooseGrid<Entity>, NoFilter> LooseGrid(int cellSize, int tightSize, int worldSize)
    {
        return new SpatialPlugin<SpatialLooseGrid<Entity>, NoFilter>(new SpatialLooseGrid<Entity>(cellSize, tightSize, worldSize));
    }
}

public class SpatialPlugin<TSpatialQuery, TQueryFilters> : IPlugin
    where TSpatialQuery : ISpatialContainer<Entity>
    where TQueryFilters : ITuple, IFilter, new()
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
            Query<Read<Transform2D>, Read<CollisionShape>>.Filter<TQueryFilters> qShapes) =>
        {
            spatialQuery.Prepare();
            qShapes.ForEach(new UpdateJob() { SpatialQuery = spatialQuery });
        }));
    }

    readonly struct UpdateJob : IEntityForEach<Read<Transform2D>, Read<CollisionShape>>
    {
        public readonly SpatialQuery SpatialQuery { get; init; }

        public readonly void Execute(in Entity entity, ref Read<Transform2D> transform, ref Read<CollisionShape> shape)
        {
            var boundingCircle = shape.Component.GetBoundingCircle(transform.Component);
            SpatialQuery.Insert(entity, transform.Component.Position, boundingCircle.Radius, ~0u);
        }
    }
}