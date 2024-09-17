namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.ECS.Core;
using Pollus.Engine.Assets;
using Pollus.Graphics.WGPU;

public class MaterialPlugin<TMaterial> : IPlugin
    where TMaterial : IMaterial
{
    public MaterialPlugin() { }

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<TMaterial>());
        world.Schedule.AddSystems(CoreStage.PreRender, new ExtractMaterialSystem<TMaterial>());
    }
}

public class ExtractMaterialSystem<TMaterial> : Sys<RenderAssets, AssetServer, IWGPUContext, Assets<TMaterial>>
    where TMaterial : IMaterial
{
    public ExtractMaterialSystem()
        : base(new SystemDescriptor(nameof(ExtractMaterialSystem<TMaterial>)))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, Assets<TMaterial> materials)
    {
        foreach (var material in materials.AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, material.Handle);
        }
    }
}