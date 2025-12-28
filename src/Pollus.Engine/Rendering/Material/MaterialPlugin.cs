namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.WGPU;

public class MaterialPlugin<TMaterial> : IPlugin
    where TMaterial : IMaterial, IAsset
{
    public static readonly string PrepareSystem = $"MaterialPlugin<{typeof(TMaterial).Name}>::PrepareSystem";
    public static readonly string ReloadSystem = $"MaterialPlugin<{typeof(TMaterial).Name}>::ReloadSystem";

    public static MaterialPlugin<TMaterial> Default => new MaterialPlugin<TMaterial>();

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Resources.Get<AssetServer>().InitAssets<TMaterial>();
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<TMaterial>());

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new(PrepareSystem)
            {
                RunsAfter = [RenderingPlugin.BeginFrameSystem],
                RunCriteria = EventRunCriteria<AssetEvent<TMaterial>>.Create,
            },
            static (RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, EventReader<AssetEvent<TMaterial>> assetEvents) =>
            {
                foreach (scoped ref readonly var assetEvent in assetEvents.Read())
                {
                    if (assetEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;

                    renderAssets.Prepare(gpuContext, assetServer, assetEvent.Handle, assetEvent.Type is AssetEventType.Changed);
                } 
            }));
    }

    public static MaterialPlugin<TMaterial> Create()
    {
        return new MaterialPlugin<TMaterial>();
    }
}