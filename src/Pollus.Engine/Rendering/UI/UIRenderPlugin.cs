namespace Pollus.Engine.Rendering;

using Pollus.Assets;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;

public class UIRenderPlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
        PluginDependency.From<UISystemsPlugin>(),
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

        var assetServer = world.Resources.Get<AssetServer>();

        var whitePixel = assetServer.GetAssets<Texture2D>().Add(new Texture2D
        {
            Name = "ui_white_pixel",
            Width = 1,
            Height = 1,
            Format = TextureFormat.Rgba8Unorm,
            Data = [255, 255, 255, 255],
        }, "internal://textures/ui_white");

        var defaultSampler = assetServer.Load<SamplerAsset>("internal://samplers/nearest");
        var linearSampler = assetServer.Load<SamplerAsset>("internal://samplers/linear");

        var rectMaterial = assetServer.GetAssets<UIRectMaterial>().Add(new UIRectMaterial
        {
            ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/ui_rect.wgsl"),
            Texture = whitePixel,
            Sampler = defaultSampler,
        });

        world.Resources.Add(new UIRenderResources
        {
            Material = rectMaterial,
            DefaultSampler = defaultSampler,
            LinearSampler = linearSampler,
        });

        world.AddPlugin(new UniformPlugin<UIUniform, Param<IWindow, Time, CurrentDevice<Mouse>>>()
        {
            Extract = static (in Param<IWindow, Time, CurrentDevice<Mouse>> param, ref UIUniform uniform) =>
            {
                var (window, time, currentMouse) = param;
                uniform.ViewportSize = new Vec2f(window.Size.X, window.Size.Y);
                uniform.Time = (float)time.SecondsSinceStartup;
                uniform.DeltaTime = time.DeltaTimeF;
                var mousePos = currentMouse.Value?.Position ?? default;
                uniform.MousePosition = new Vec2f(mousePos.X, mousePos.Y);
                uniform.Scale = 1.0f;
            }
        });

        world.Schedule.AddSystemSet<ExtractUIRectsSystem>();
    }
}
