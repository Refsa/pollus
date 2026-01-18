namespace Pollus.Engine.Rendering;

using Graphics;
using Pollus.ECS;
using Transform;

public class SpritePlugin : IPlugin
{
    public static SpritePlugin Default => new();

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

        {
            var batches = new SpriteBatches()
            {
                RendererKey = RendererKey.From<SpriteBatches>(),
            };
            var registry = world.Resources.Get<RenderQueueRegistry>();
            registry.Register(batches.RendererKey, batches);
            world.Resources.Add(batches);
        }

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractSpritesSystem(),
        ]);
    }
}