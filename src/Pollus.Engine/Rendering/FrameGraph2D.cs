namespace Pollus.Engine.Rendering;

using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Utils;

public struct FrameGraph2DParam
{
    public required RenderAssets RenderAssets { get; init; }
    public required DrawGroups2D DrawGroups;
    public required Resources Resources;
    public required TextureFormat BackbufferFormat;
    public required Vec2<uint> BackbufferSize;
}

public struct PrepareTexturesPass
{
    public ResourceHandle<TextureResource> ColorAttachment;
}

public struct FinalBlitPass
{
    public ResourceHandle<TextureResource> ColorAttachment;
    public ResourceHandle<TextureResource> Backbuffer;
}

public class FrameGraph2D : IDisposable
{
    public static class Textures
    {
        public const string Backbuffer = "backbuffer";
        public const string ColorTarget = "color-target";
    }

    bool frameStarted;
    FrameGraph<FrameGraph2DParam> frameGraph;
    FrameGraph2DParam param;
    bool isDisposed;

    public ref readonly FrameGraph2DParam Param => ref param;
    public ref FrameGraph<FrameGraph2DParam> FrameGraph => ref frameGraph;
    public bool FrameStarted => frameStarted;

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);
        Cleanup();
    }

    public void BeginFrame(FrameGraph<FrameGraph2DParam> frameGraph, FrameGraph2DParam param)
    {
        frameStarted = true;
        this.frameGraph = frameGraph;
        this.param = param;
    }

    public FrameGraphRunner<FrameGraph2DParam> Compile()
    {
        if (!frameStarted)
            throw new InvalidOperationException("Frame not started");

        frameStarted = false;
        return frameGraph.Compile();
    }

    public void AddPass<TData>(RenderStep2D step, FrameGraph<FrameGraph2DParam>.BuilderDelegate<TData> builder, FrameGraph<FrameGraph2DParam>.ExecuteDelegate<TData> executor)
        where TData : struct
    {
        if (!frameStarted)
            throw new InvalidOperationException("Frame not started");

        frameGraph.AddPass(step, param, builder, executor);
    }

    public void Cleanup()
    {
        if (Unsafe.IsNullRef(ref frameGraph)) return;
        frameGraph.Dispose();
        Unsafe.SkipInit(out frameGraph);
    }
}

public class FrameGraph2DPlugin : IPlugin
{
    public const string BeginFrame = "RenderPipeline::BeginFrame";
    public const string Render = "RenderPipeline::Render";

    public void Apply(World world)
    {
        world.Resources.Add(new FrameGraph2D());

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new(BeginFrame)
        {
            RunsAfter = [RenderingPlugin.BeginFrameSystem]
        },
        static (RenderContext renderContext, RenderAssets renderAssets,
                DrawGroups2D drawGroups, Resources resources,
                IWindow window, FrameGraph2D renderGraph) =>
        {
            var param = new FrameGraph2DParam()
            {
                RenderAssets = renderAssets,
                DrawGroups = drawGroups,
                Resources = resources,
                BackbufferFormat = renderContext.SurfaceTextureView!.Value.Descriptor.Format,
                BackbufferSize = window.Size,
            };

            var frameGraph = new FrameGraph<FrameGraph2DParam>();
            var backbufferDesc = TextureDescriptor.D2(
                FrameGraph2D.Textures.Backbuffer,
                TextureUsage.RenderAttachment | TextureUsage.TextureBinding,
                param.BackbufferFormat,
                param.BackbufferSize
            );
            var backbufferHandle = frameGraph.AddTexture(new(FrameGraph2D.Textures.Backbuffer, backbufferDesc));
            renderContext.Resources.SetTexture(backbufferHandle, new(null, renderContext.SurfaceTextureView!.Value, backbufferDesc));

            frameGraph.AddPass(RenderStep2D.First, param,
            static (ref FrameGraph<FrameGraph2DParam>.Builder builder, FrameGraph2DParam param, ref PrepareTexturesPass data) =>
            {
                data.ColorAttachment = builder.Creates<TextureResource>(TextureDescriptor.D2(
                    FrameGraph2D.Textures.ColorTarget,
                    TextureUsage.RenderAttachment | TextureUsage.TextureBinding,
                    param.BackbufferFormat,
                    param.BackbufferSize
                ));
            },
            static (context, param, data) =>
            {
                var commandEncoder = context.GetCurrentCommandEncoder();
                var colorTexture = context.Resources.GetTexture(data.ColorAttachment);

                param.RenderAssets.Get(Blit.Handle).ClearTexture(
                    context.GPUContext, commandEncoder,
                    colorTexture.TextureView, new Color(0.1f, 0.1f, 0.1f, 1.0f));
            });

            frameGraph.AddPass(RenderStep2D.Main, param,
            static (ref FrameGraph<FrameGraph2DParam>.Builder builder, FrameGraph2DParam param, ref MainPass data) =>
            {
                data.ColorAttachment = builder.Reads<TextureResource>(FrameGraph2D.Textures.ColorTarget);
            },
            static (context, param, data) =>
            {
                var commandEncoder = context.GetCurrentCommandEncoder();
                using var passEncoder = commandEncoder.BeginRenderPass(new()
                {
                    ColorAttachments = stackalloc RenderPassColorAttachment[]
                    {
                        new()
                        {
                            View = context.Resources.GetTexture(data.ColorAttachment).TextureView.Native,
                            LoadOp = LoadOp.Load,
                            StoreOp = StoreOp.Store,
                        }
                    }
                });

                var stage = param.DrawGroups.Groups[RenderStep2D.Main];
                stage.Execute(passEncoder, param.RenderAssets);
            });

            frameGraph.AddPass(RenderStep2D.UI, param,
            static (ref FrameGraph<FrameGraph2DParam>.Builder builder, FrameGraph2DParam param, ref UIPass data) =>
            {
                data.ColorAttachment = builder.Reads<TextureResource>(FrameGraph2D.Textures.ColorTarget);
            },
            static (context, param, data) =>
            {
                var commandEncoder = context.GetCurrentCommandEncoder();
                using var passEncoder = commandEncoder.BeginRenderPass(new()
                {
                    ColorAttachments = stackalloc RenderPassColorAttachment[]
                    {
                        new()
                        {
                            View = context.Resources.GetTexture(data.ColorAttachment).TextureView.Native,
                            LoadOp = LoadOp.Load,
                            StoreOp = StoreOp.Store,
                        }
                    }
                });

                var stage = param.DrawGroups.Groups[RenderStep2D.UI];
                stage.Execute(passEncoder, param.RenderAssets);
            });

            frameGraph.AddPass(RenderStep2D.Last, param,
            static (ref FrameGraph<FrameGraph2DParam>.Builder builder, FrameGraph2DParam param, ref FinalBlitPass data) =>
            {
                data.ColorAttachment = builder.Reads<TextureResource>(FrameGraph2D.Textures.ColorTarget);
                data.Backbuffer = builder.Reads<TextureResource>(FrameGraph2D.Textures.Backbuffer);
            },
            static (context, param, data) =>
            {
                var commandEncoder = context.GetCurrentCommandEncoder();
                var colorTexture = context.Resources.GetTexture(data.ColorAttachment);
                var backbufferTexture = context.Resources.GetTexture(data.Backbuffer);

                param.RenderAssets.Get(Blit.Handle).BlitTexture(
                    context.GPUContext, commandEncoder,
                    colorTexture.TextureView, backbufferTexture.TextureView, Color.BLACK);
            });

            renderGraph.BeginFrame(frameGraph, param);
        }));

        world.Schedule.AddSystems(CoreStage.Render, FnSystem.Create(Render,
        static (FrameGraph2D renderGraph, RenderContext context) =>
        {
            renderGraph.Compile().Execute(context, renderGraph.Param);
        }));
    }
}