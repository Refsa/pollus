namespace Pollus.Engine.Rendering;

using Pollus.ECS;

public class SpritePlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(new SpriteBatches());
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<SpriteMaterial>());

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractSpritesSystem(),
            new WriteSpriteBatchesSystem(),
            new DrawSpriteBatchesSystem(),
        ]);
    }
}