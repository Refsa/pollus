namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Transform;

public class SpritePlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
        PluginDependency.From<TransformPlugin<Transform2D>>(),
    ];

    public void Apply(World world)
    {
        world.AddPlugins(true, [
            new MaterialPlugin<SpriteMaterial>(),
        ]);
        world.Resources.Add(new SpriteBatches()
        {
            RendererID = RendererKey.From<SpriteBatches>().Key,
        });

        var registry = world.Resources.Get<RenderQueueRegistry>();
        registry.Register(world.Resources.Get<SpriteBatches>().RendererID, world.Resources.Get<SpriteBatches>());

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractSpritesSystem(),
        ]);
    }
}