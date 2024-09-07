namespace Pollus.Tests.Graphics;

using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;

public class FrameGraphTests
{
    struct PassData1
    {
        public ResourceHandle<TextureDescriptor> Texture1;
    }

    struct PassData2
    {
        public ResourceHandle<TextureDescriptor> Texture1;
    }

    struct PassData3
    {
        public ResourceHandle<TextureDescriptor> Texture1;
    }

    [Fact]
    public void FrameGraph_Compile()
    {
        var frameGraph = new FrameGraph<object>();
        var texture1Handle = frameGraph.AddResource(TextureDescriptor.D2(
            label: "texture1",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.CopySrc | TextureUsage.RenderAttachment
        ));

        frameGraph.AddPass(
            "pass1",
            (ref FrameGraph<object>.Builder builder, ref PassData1 data) =>
            {
                data.Texture1 = builder.Reads("texture1");
                Assert.Equal(texture1Handle, (ResourceHandle)data.Texture1);
            },
            (context, renderAssets, data) => { }
        );

        frameGraph.AddPass(
            "pass2",
            (ref FrameGraph<object>.Builder builder, ref PassData2 data) =>
            {
                data.Texture1 = builder.Writes("texture1");
                Assert.Equal(texture1Handle, (ResourceHandle)data.Texture1);
            },
            (context, renderAssets, data) => {}
        );

        frameGraph.AddPass(
            "pass3",
            (ref FrameGraph<object>.Builder builder, ref PassData3 data) =>
            {
                data.Texture1 = builder.Writes("texture1");
                Assert.Equal(texture1Handle, (ResourceHandle)data.Texture1);
            },
            (context, renderAssets, data) => { }
        );

        var runner = frameGraph.Compile();
        Assert.Equal(3, runner.order.Length);
        Assert.Equal(1, runner.order[0]);
        Assert.Equal(2, runner.order[1]);
        Assert.Equal(0, runner.order[2]);
    }
}