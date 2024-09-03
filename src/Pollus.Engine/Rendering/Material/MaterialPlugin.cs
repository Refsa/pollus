namespace Pollus.Engine.Rendering;

using Pollus.ECS;

public class MaterialPlugin<TMaterial> : IPlugin
    where TMaterial : IMaterial
{
    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<TMaterial>());
        world.Schedule.AddSystems(CoreStage.PreRender, new ExtractRenderablesSystem<TMaterial>());
    }
}