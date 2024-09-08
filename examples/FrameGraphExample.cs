namespace Pollus.Examples;

using Pollus.Engine.Rendering;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
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
        var frameGraph = new FrameGraph<RenderAssets>();
        frameGraph.AddTexture(TextureDescriptor.D2(
            label: "backbuffer",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.CopySrc | TextureUsage.RenderAttachment
        ));

        frameGraph.AddPass("sprites-pass",
        static (ref FrameGraph<RenderAssets>.Builder builder, ref SpritesPassData data) =>
        {
            data.ColorTexture = builder.Writes<TextureResource>("backbuffer");
        }, 
        static (context, renderAssets, data) =>
        {
            // Execute pass
        });

        frameGraph.AddPass("blit-pass",
        static (ref FrameGraph<RenderAssets>.Builder builder, ref BlitPassData data) =>
        {

        },
        static (context, renderAssets, data) =>
        {
            // Execute pass
        });

        frameGraph.Compile();
        // frameGraph.Execute(null, null);
    }

    public void Stop()
    {

    }
}