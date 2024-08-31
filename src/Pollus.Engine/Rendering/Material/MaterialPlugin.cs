namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;

public class MaterialPlugin<TMaterial> : IPlugin
    where TMaterial : IMaterial
{
    static MaterialPlugin()
    {
        AssetsFetch<Material>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<TMaterial>());
        world.Schedule.AddSystems(CoreStage.PreRender, new ExtractRenderablesSystem<TMaterial>());
    }
}