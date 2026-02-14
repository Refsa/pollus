namespace Pollus.Engine.Rendering;

using Graphics.WGPU;
using Pollus.ECS;
using Pollus.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;

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

        {
            var batches = new ShapeBatches()
            {
                RendererKey = RendererKey.From<ShapeBatches>(),
            };
            var registry = world.Resources.Get<RenderQueueRegistry>();
            registry.Register(batches.RendererKey, batches);
            world.Resources.Add(batches);
        }

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