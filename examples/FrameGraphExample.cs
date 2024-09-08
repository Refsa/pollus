namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Debug;
using Pollus.Engine.Rendering;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;

public class FrameGraphExample : IExample
{
    public string Name => "frame-graph";

    struct SpritesPassData
    {
        public ResourceHandle<TextureResource> ColorTexture;
    }

    struct BlitPassData
    {
        public ResourceHandle<TextureResource> Backbuffer;
    }

    public void Run()
    {
        Application.Builder
            .AddPlugins([
                new AssetPlugin() { RootPath = "assets" },
                new RenderingPlugin(),
                new PerformanceTrackerPlugin(),
            ])
            .AddSystem(CoreStage.PreRender, SystemBuilder.FnSystem("FrameGraph",
            static (RenderContext renderContext, RenderAssets renderAssets, IWindow window) =>
            {
                var frameGraph = new FrameGraph<RenderAssets>();
                frameGraph.AddTexture(TextureDescriptor.D2(
                    label: "backbuffer",
                    size: window.Size,
                    format: TextureFormat.Rgba8Unorm,
                    usage: TextureUsage.RenderAttachment
                ));

                frameGraph.AddPass("sprites-pass",
                static (ref FrameGraph<RenderAssets>.Builder builder, ref SpritesPassData data) =>
                {
                    data.ColorTexture = builder.Writes<TextureResource>("backbuffer");
                },
                static (context, renderAssets, data) =>
                {
                    
                });

                var runner = frameGraph.Compile();
                runner.Execute(renderContext, renderAssets);
            }))
            .Run();
    }

    public void Stop()
    {

    }
}