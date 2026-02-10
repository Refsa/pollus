namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.UI;

public class UIRenderPlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
        PluginDependency.From<UIPlugin>(),
    ];

    public void Apply(World world)
    {
        world.AddPlugin(new MaterialPlugin<UIRectMaterial>());

        {
            var batches = new UIRectBatches()
            {
                RendererKey = RendererKey.From<UIRectBatches>(),
            };
            var registry = world.Resources.Get<RenderQueueRegistry>();
            registry.Register(batches.RendererKey, batches);
            world.Resources.Add(batches);
        }

        world.Resources.Add(default(UIRenderResources));

        world.AddPlugin(new UniformPlugin<UIViewportUniform, Param<IWindow>>()
        {
            Extract = static (in Param<IWindow> param, ref UIViewportUniform uniform) =>
            {
                var (window, _) = param;
                uniform.ViewportSize = new Vec2f(window.Size.X, window.Size.Y);
            }
        });

        world.Schedule.AddSystems(CoreStage.PreRender, ExtractUIRectsSystem.Create());
    }
}
