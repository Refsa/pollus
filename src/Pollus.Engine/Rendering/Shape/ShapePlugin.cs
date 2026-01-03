namespace Pollus.Engine.Rendering;

using Graphics.WGPU;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public class ShapePlugin : IPlugin
{
    static ShapePlugin()
    {
        AssetsFetch<Shape>.Register();
    }

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
        PluginDependency.From<TransformPlugin<Transform2D>>(),
    ];

    public void Apply(World world)
    {
        world.AddPlugin(new MaterialPlugin<ShapeMaterial>());
        world.Resources.Add(new ShapeBatches());

        var registry = world.Resources.Get<RenderQueueRegistry>();
        var batches = world.Resources.Get<ShapeBatches>();
        registry.Register(RendererKey.From<ShapeBatches>().Key, batches);
        world.Resources.Get<RenderAssets>().AddLoader(new ShapeRenderDataLoader());

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractShapeDrawSystem(),
        ]);

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new("ShapePlugin::PrepareShapeAssets")
            {
                RunsAfter = [RenderingPlugin.BeginFrameSystem],
                RunCriteria = EventRunCriteria<AssetEvent<Shape>>.Create,
            },
            static (IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets, EventReader<AssetEvent<Shape>> assetEvents) =>
            {
                foreach (scoped ref readonly var assetEvent in assetEvents.Read())
                {
                    if (assetEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;

                    renderAssets.Prepare(gpuContext, assetServer, assetEvent.Handle, assetEvent.Type is AssetEventType.Changed);
                }
            }));
    }
}

public partial struct ShapeDraw : IComponent
{
    public static EntityBuilder<ShapeDraw, Transform2D, GlobalTransform> Bundle => new(
        new()
        {
            MaterialHandle = Handle<ShapeMaterial>.Null,
            ShapeHandle = Handle<Shape>.Null,
            Color = Color.WHITE,
            Offset = Vec2f.Zero,
        },
        Transform2D.Default,
        GlobalTransform.Default
    );

    public required Handle<ShapeMaterial> MaterialHandle;
    public required Handle<Shape> ShapeHandle;
    public required Color Color;
    public Vec2f Offset;
}
