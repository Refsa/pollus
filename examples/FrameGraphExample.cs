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
        public ResourceHandle<TextureFrameResource> ColorTexture;
    }

    struct BlitPassData
    {
        public ResourceHandle<TextureFrameResource> Backbuffer;
    }

    public void Run()
    {
        var frameGraph = new FrameGraph<RenderAssets>();
        frameGraph.AddTextureResource(new TextureFrameResource("backbuffer", TextureDescriptor.D2(
            label: "backbuffer",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.CopySrc | TextureUsage.RenderAttachment
        )));

        frameGraph.AddPass<SpritesPassData>("sprites-pass",
        static (builder, data) =>
        {
            data.ColorTexture = builder.Creates("color-texture", TextureDescriptor.D2(
                label: "color-texture",
                size: new Vec2<uint>(800, 600),
                format: TextureFormat.Rgba8Unorm,
                usage: TextureUsage.CopyDst | TextureUsage.RenderAttachment
            ));
        }, 
        static (context, renderAssets, data) =>
        {
            // Execute pass
        });

        frameGraph.AddPass<BlitPassData>("blit-pass",
        static (builder, data) =>
        {
            data.Backbuffer = builder.Writes(builder.LookupTexture("backbuffer"));
        },
        static (context, renderAssets, data) =>
        {
            // Execute pass
        });

        frameGraph.Compile().Execute(null);
    }

    public void Stop()
    {

    }
}