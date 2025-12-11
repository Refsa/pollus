namespace Pollus.Tests.Graphics;

using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;

public class FrameGraphTests
{
    struct PassData1
    {
        public ResourceHandle<TextureResource> Texture1;
    }

    struct PassData2
    {
        public ResourceHandle<TextureResource> Texture1;
    }

    struct PassData3
    {
        public ResourceHandle<TextureResource> Texture1;
    }

    struct PassData4
    {
        public ResourceHandle<TextureResource> Texture1;
    }

    enum PassOrder
    {
        First = 0,
        Second = 100,
        Last = 200,
    }

    [Fact]
    public void FrameGraph_Compile()
    {
        var param = new object();

        using var frameGraph = new FrameGraph<object>();
        var texture1Handle = frameGraph.AddTexture(TextureDescriptor.D2(
            label: "texture1",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.CopySrc | TextureUsage.RenderAttachment
        ));

        frameGraph.AddPass(PassOrder.Second, param,
            (ref FrameGraph<object>.Builder builder, in object param, ref PassData1 data) =>
            {
                data.Texture1 = builder.Reads<TextureResource>("texture1");
                Assert.Equal(texture1Handle, data.Texture1);
            },
            (context, in renderAssets, in data) => { }
        );

        frameGraph.AddPass(PassOrder.Second, param,
            (ref FrameGraph<object>.Builder builder, in object param, ref PassData2 data) =>
            {
                data.Texture1 = builder.Writes<TextureResource>("texture1");
                Assert.Equal(texture1Handle, data.Texture1);
            },
            (context, in renderAssets, in data) => { }
        );

        frameGraph.AddPass(PassOrder.Second, param,
            (ref FrameGraph<object>.Builder builder, in object param, ref PassData3 data) =>
            {
                data.Texture1 = builder.Writes<TextureResource>("texture1");
                Assert.Equal(texture1Handle, data.Texture1);
            },
            (context, in renderAssets, in data) => { }
        );

        var runner = frameGraph.Compile();
        File.WriteAllText("frame-graph.dot", frameGraph.Visualize());

        Assert.Equal(3, runner.order.Length);
        Assert.Equal(2, runner.order[0]);
        Assert.Equal(1, runner.order[1]);
        Assert.Equal(0, runner.order[2]);
    }

    [Fact]
    public void FrameGraph_Order()
    {
        var param = new object();
        using var frameGraph = new FrameGraph<object>();

        frameGraph.AddPass(PassOrder.Last, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData1 data) => { },
            static (context, in renderAssets, in data) => { }
        );

        frameGraph.AddPass(PassOrder.First, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData2 data) => { },
            static (context, in renderAssets, in data) => { }
        );

        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData4 data) => { },
            static (context, in renderAssets, in data) => { }
        );

        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData3 data) => { },
            static (context, in renderAssets, in data) => { }
        );


        var runner = frameGraph.Compile();
        Assert.Equal(4, runner.order.Length);

        Assert.Equal(1, runner.order[0]);
        Assert.Equal(3, runner.order[1]);
        Assert.Equal(2, runner.order[2]);
        Assert.Equal(0, runner.order[3]);
    }

    [Fact]
    public void FrameGraph_OrderWithReadWrite()
    {
        var param = new object();
        using var frameGraph = new FrameGraph<object>();
        frameGraph.AddTexture(TextureDescriptor.D2(
            label: "texture1",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.CopySrc | TextureUsage.RenderAttachment
        ));

        frameGraph.AddPass(PassOrder.Last, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData1 data) => { },
            static (context, in renderAssets, in data) => { }
        );

        frameGraph.AddPass(PassOrder.First, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData2 data) => { },
            static (context, in renderAssets, in data) => { }
        );

        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData3 data) =>
            {
                data.Texture1 = builder.Writes<TextureResource>("texture1");
            },
            static (context, in renderAssets, in data) => { }
        );

        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData4 data) =>
            {
                data.Texture1 = builder.Reads<TextureResource>("texture1");
            },
            static (context, in renderAssets, in data) => { }
        );

        var runner = frameGraph.Compile();
        Assert.Equal(4, runner.order.Length);

        Assert.Equal(1, runner.order[0]);
        Assert.Equal(2, runner.order[1]);
        Assert.Equal(3, runner.order[2]);
        Assert.Equal(0, runner.order[3]);
    }
}