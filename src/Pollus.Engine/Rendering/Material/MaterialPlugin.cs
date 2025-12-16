namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.WGPU;

public class MaterialPlugin<TMaterial> : IPlugin
    where TMaterial : IMaterial
{
    public static MaterialPlugin<TMaterial> Default => new MaterialPlugin<TMaterial>();

    public MaterialPlugin() { }

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<TMaterial>());
        world.Schedule.AddSystems(CoreStage.PreRender, new ExtractMaterialSystem<TMaterial>());
    }

    public static MaterialPlugin<TMaterial> Create()
    {
        return new MaterialPlugin<TMaterial>();
    }
}

public class ExtractMaterialSystem<TMaterial> : SystemBase<RenderAssets, AssetServer, IWGPUContext, Assets<TMaterial>>
    where TMaterial : IMaterial
{
    public ExtractMaterialSystem()
        : base(new SystemDescriptor($"ExtractMaterialSystem::<{typeof(TMaterial).Name}>"))
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