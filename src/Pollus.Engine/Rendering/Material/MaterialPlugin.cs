namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.WGPU;

public class MaterialPlugin<TMaterial> : IPlugin
    where TMaterial : IMaterial
{
    public static MaterialPlugin<TMaterial> Default => new MaterialPlugin<TMaterial>();

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
    ];

    public MaterialPlugin() { }

    public void Apply(World world)
    {
        world.Resources.Get<AssetServer>().InitAsset<TMaterial>();

        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<TMaterial>());

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new($"MaterialPlugin<{typeof(TMaterial).Name}>::PrepareSystem")
            {
                RunsAfter = [RenderingPlugin.BeginFrameSystem],
                RunCriteria = EventRunCriteria<AssetEvent<TMaterial>>.Create,
            },
            static (RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, EventReader<AssetEvent<TMaterial>> assetEvents) =>
            {
                foreach (scoped ref readonly var assetEvent in assetEvents.Read())
                {
                    if (assetEvent.Type is AssetEventType.Removed) continue;

                    renderAssets.Prepare(gpuContext, assetServer, assetEvent.Handle, assetEvent.Type is AssetEventType.Changed);
                }
            }));
    }

    public static MaterialPlugin<TMaterial> Create()
    {
        return new MaterialPlugin<TMaterial>();
    }
}