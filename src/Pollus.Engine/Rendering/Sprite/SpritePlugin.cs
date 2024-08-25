namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;

public class SpritePlugin : IPlugin
{
    static SpritePlugin()
    {
        AssetsFetch<SpriteMaterial>.Register();
        ResourceFetch<SpriteBatches>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Add(new SpriteBatches());
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<SpriteMaterial>());
        
        world.Schedule.AddSystems(CoreStage.PostInit, SystemBuilder.FnSystem("InitSpritePlugin",
            static (RenderSteps renderSteps) => renderSteps.Add(new SpriteBatchDraw())
        ));
        world.Schedule.AddSystems(CoreStage.PreRender, new ExtractSpritesSystem());
    }
}