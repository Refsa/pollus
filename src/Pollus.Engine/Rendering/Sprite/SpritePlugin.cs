namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
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
        world.Resources.Add(new SpriteBatches());

        var registry = world.Resources.Get<RenderQueueRegistry>();
        var batches = world.Resources.Get<SpriteBatches>();
        registry.Register(RendererKey.From<SpriteBatches>().Key, batches);

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractSpritesSystem(),
        ]);
    }
}