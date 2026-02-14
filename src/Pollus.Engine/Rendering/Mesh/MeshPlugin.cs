namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.ECS;
using Pollus.Assets;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;

[Asset]
public partial class MeshAsset
{
    public required string Name { get; init; }
    public required Mesh Mesh { get; init; }
}

public class MeshPlugin : IPlugin
{
    static MeshPlugin()
    {
        ResourceFetch<MeshRenderBatches>.Register();
    }

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From(() => AssetPlugin.Default),
        PluginDependency.From<RenderingPlugin>(),
    ];

    public PrimitiveType SharedPrimitives { get; init; }

    public void Apply(World world)
    {
        world.Resources.Init<MeshAsset>();

        world.Resources.Get<RenderAssets>().AddLoader(new MeshRenderDataLoader());

        {
            var batches = new MeshRenderBatches()
            {
                RendererKey = RendererKey.From<MeshRenderBatches>(),
            };
            var registry = world.Resources.Get<RenderQueueRegistry>();
            registry.Register(batches.RendererKey, batches);
            world.Resources.Add(batches);
        }

        if (SharedPrimitives != PrimitiveType.None)
        {
            var assetServer = world.Resources.Get<AssetServer>();

            var primitives = new PrimitiveMeshes();
            primitives.InitPrimitives(SharedPrimitives, assetServer.GetAssets<MeshAsset>());
            world.Resources.Add(primitives);
        }

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(
            new("MeshPlugin::PrepareMeshAssets")
            {
                RunsAfter = [RenderingPlugin.BeginFrameSystem],
                RunCriteria = EventRunCriteria<AssetEvent<MeshAsset>>.Create,
            },
            static (IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets, EventReader<AssetEvent<MeshAsset>> assetEvents) =>
            {
                foreach (scoped ref readonly var assetEvent in assetEvents.Read())
                {
                    if (assetEvent.Type is AssetEventType.Unloaded) continue;

                    renderAssets.Prepare(gpuContext, assetServer, assetEvent.Handle, assetEvent.Type is AssetEventType.Changed);
                }
            }
        ));
    }
}
