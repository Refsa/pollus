namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.Engine.Physics;
using Pollus.Engine.Transform;
using Pollus.Mathematics;

public static class SpatialPlugin
{
    public static SpatialPlugin<SpatialHashGrid<Entity>, NoFilter> Grid(int cellSize, int width, int height, Vec2f? offset = null)
    {
        return new SpatialPlugin<SpatialHashGrid<Entity>, NoFilter>(new SpatialHashGrid<Entity>(cellSize, width, height), offset ?? Vec2f.Zero);
    }

    public static SpatialPlugin<SpatialHashGrid<Entity>, TQueryFilters> Grid<TQueryFilters>(int cellSize, int width, int height, Vec2f? offset = null)
        where TQueryFilters : ITuple, IFilter, new()
    {
        return new SpatialPlugin<SpatialHashGrid<Entity>, TQueryFilters>(new SpatialHashGrid<Entity>(cellSize, width, height), offset ?? Vec2f.Zero);
    }

    public static SpatialPlugin<SpatialLooseGrid<Entity>, NoFilter> LooseGrid(int cellSize, int tightSize, int worldSize, Vec2f? offset = null)
    {
        return new SpatialPlugin<SpatialLooseGrid<Entity>, NoFilter>(new SpatialLooseGrid<Entity>(cellSize, tightSize, worldSize), offset ?? Vec2f.Zero);
    }
}

public record SpatialPlugin<TSpatialQuery, TQueryFilters> : IPlugin
    where TSpatialQuery : ISpatialContainer<Entity>
    where TQueryFilters : ITuple, IFilter, new()
{
    public TSpatialQuery SpatialQuery { get; init; }
    public Vec2f Offset { get; init; }
    public bool IsStatic { get; init; }

    public SpatialPlugin(TSpatialQuery spatialQuery, Vec2f offset)
    {
        SpatialQuery = spatialQuery;
        Offset = offset;
    }

    public void Apply(World world)
    {
        if (world.Resources.Has<SpatialQuery>())
        {
            throw new InvalidOperationException("SpatialQuery already created");
        }

        world.Resources.Add(new SpatialQuery(SpatialQuery));

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(new($"SpatialQuery<{typeof(TSpatialQuery).Name}>::Update")
        {
            Locals = [Local.From(Offset), Local.From((IsStatic, false))],
        },
        static (
            Local<Vec2f> offset,
            Local<(bool isStatic, bool isCalculated)> staticInfo,
            SpatialQuery spatialQuery,
            Query<Read<Transform2D>, Read<CollisionShape>>.Filter<(TQueryFilters, None<Layer>)> qShapes,
            Query<Read<Transform2D>, Read<CollisionShape>, Read<Layer>>.Filter<TQueryFilters> qShapesWithLayer
        ) =>
        {
            if (staticInfo.Value.isStatic && staticInfo.Value.isCalculated) return;

            spatialQuery.Prepare();
            qShapes.ForEach(new UpdateJob() { SpatialQuery = spatialQuery, Offset = offset.Value });
            qShapesWithLayer.ForEach(new UpdateWithLayerJob() { SpatialQuery = spatialQuery, Offset = offset.Value });
            staticInfo.Value = (staticInfo.Value.isStatic, true);
        }));
    }

    readonly struct UpdateJob : IEntityForEach<Read<Transform2D>, Read<CollisionShape>>
    {
        public readonly SpatialQuery SpatialQuery { get; init; }
        public readonly Vec2f Offset { get; init; }

        public readonly void Execute(in Entity entity, ref Read<Transform2D> transform, ref Read<CollisionShape> shape)
        {
            var boundingCircle = shape.Component.GetBoundingCircle(transform.Component);
            SpatialQuery.Insert(entity, transform.Component.Position + Offset, boundingCircle.Radius, ~0u);
        }
    }

    readonly struct UpdateWithLayerJob : IEntityForEach<Read<Transform2D>, Read<CollisionShape>, Read<Layer>>
    {
        public readonly SpatialQuery SpatialQuery { get; init; }
        public readonly Vec2f Offset { get; init; }

        public readonly void Execute(in Entity entity, ref Read<Transform2D> transform, ref Read<CollisionShape> shape, ref Read<Layer> layer)
        {
            var boundingCircle = shape.Component.GetBoundingCircle(transform.Component);
            SpatialQuery.Insert(entity, transform.Component.Position + Offset, boundingCircle.Radius, layer.Component.Value);
        }
    }
}