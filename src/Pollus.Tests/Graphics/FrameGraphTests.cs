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

    [Fact]
    public void FrameGraph_WAR_ReaderBeforeWriter()
    {
        var param = new object();
        using var frameGraph = new FrameGraph<object>();
        frameGraph.AddTexture(TextureDescriptor.D2(
            label: "texture1",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.CopySrc | TextureUsage.RenderAttachment
        ));

        // Pass index 0 - reads texture, PassOrder.First
        frameGraph.AddPass(PassOrder.First, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData1 data) =>
            {
                data.Texture1 = builder.Reads<TextureResource>("texture1");
            },
            static (context, in renderAssets, in data) => { }
        );

        // Pass index 1 - writes texture, PassOrder.Second
        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData2 data) =>
            {
                data.Texture1 = builder.Writes<TextureResource>("texture1");
            },
            static (context, in renderAssets, in data) => { }
        );

        var runner = frameGraph.Compile();
        Assert.Equal(2, runner.order.Length);
        Assert.Equal(0, runner.order[0]);
        Assert.Equal(1, runner.order[1]);
    }

    [Fact]
    public void FrameGraph_WAW_WriterOrderByPassOrder()
    {
        var param = new object();
        using var frameGraph = new FrameGraph<object>();
        frameGraph.AddTexture(TextureDescriptor.D2(
            label: "texture1",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.CopySrc | TextureUsage.RenderAttachment
        ));

        // Pass index 0 - writes texture, PassOrder.Second
        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData1 data) =>
            {
                data.Texture1 = builder.Writes<TextureResource>("texture1");
            },
            static (context, in renderAssets, in data) => { }
        );

        // Pass index 1 - writes texture, PassOrder.First
        frameGraph.AddPass(PassOrder.First, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData2 data) =>
            {
                data.Texture1 = builder.Writes<TextureResource>("texture1");
            },
            static (context, in renderAssets, in data) => { }
        );

        var runner = frameGraph.Compile();
        Assert.Equal(2, runner.order.Length);
        Assert.Equal(1, runner.order[0]);
        Assert.Equal(0, runner.order[1]);
    }

    [Fact]
    public void FrameGraph_FullPipeline_CreateWriteWriteRead()
    {
        var param = new object();
        using var frameGraph = new FrameGraph<object>();

        // Pass index 0 - creates+writes color target, PassOrder.First
        frameGraph.AddPass(PassOrder.First, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData1 data) =>
            {
                data.Texture1 = builder.Creates<TextureResource>(TextureDescriptor.D2(
                    label: "color-target",
                    size: new Vec2<uint>(800, 600),
                    format: TextureFormat.Rgba8Unorm,
                    usage: TextureUsage.RenderAttachment | TextureUsage.TextureBinding
                ));
            },
            static (context, in renderAssets, in data) => { }
        );

        // Pass index 1 - writes color target, PassOrder.Second
        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData2 data) =>
            {
                data.Texture1 = builder.Writes<TextureResource>("color-target");
            },
            static (context, in renderAssets, in data) => { }
        );

        // Pass index 2 - writes color target, PassOrder.Second (same as above)
        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData3 data) =>
            {
                data.Texture1 = builder.Writes<TextureResource>("color-target");
            },
            static (context, in renderAssets, in data) => { }
        );

        // Pass index 3 - reads color target, PassOrder.Last
        frameGraph.AddPass(PassOrder.Last, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData4 data) =>
            {
                data.Texture1 = builder.Reads<TextureResource>("color-target");
            },
            static (context, in renderAssets, in data) => { }
        );

        var runner = frameGraph.Compile();
        Assert.Equal(4, runner.order.Length);

        Assert.Equal(0, runner.order[0]); // First - creates
        Assert.Contains(1, runner.order[1..3].ToArray());
        Assert.Contains(2, runner.order[1..3].ToArray());
        Assert.Equal(3, runner.order[3]); // Last - reads
    }

    [Fact]
    public void FrameGraph_CycleDetection_SamePassOrder()
    {
        var param = new object();
        using var frameGraph = new FrameGraph<object>();
        frameGraph.AddTexture(TextureDescriptor.D2(
            label: "tex-a",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.RenderAttachment
        ));
        frameGraph.AddTexture(TextureDescriptor.D2(
            label: "tex-b",
            size: new Vec2<uint>(800, 600),
            format: TextureFormat.Rgba8Unorm,
            usage: TextureUsage.RenderAttachment
        ));

        // Pass 0 writes tex-a, reads tex-b
        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData1 data) =>
            {
                builder.Writes<TextureResource>("tex-a");
                builder.Reads<TextureResource>("tex-b");
            },
            static (context, in renderAssets, in data) => { }
        );

        // Pass 1 writes tex-b, reads tex-a → creates a cycle with pass 0
        frameGraph.AddPass(PassOrder.Second, param,
            static (ref FrameGraph<object>.Builder builder, in object param, ref PassData2 data) =>
            {
                builder.Writes<TextureResource>("tex-b");
                builder.Reads<TextureResource>("tex-a");
            },
            static (context, in renderAssets, in data) => { }
        );

        Assert.Throws<Exception>(() => frameGraph.Compile());
    }
}