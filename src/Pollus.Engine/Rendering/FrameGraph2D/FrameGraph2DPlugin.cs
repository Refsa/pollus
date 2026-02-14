namespace Pollus.Engine.Rendering;

using ECS;
using Graphics;
using Graphics.Rendering;
using Graphics.Windowing;
using Utils;
using Pollus.Platform.Window;

public class FrameGraph2DPlugin : IPlugin
{
    public const string BeginFrame = "FrameGraph2D::BeginFrame";
    public const string Render = "FrameGraph2D::Render";
    public const string Cleanup = "FrameGraph2D::Cleanup";

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<WindowPlugin>(),
        PluginDependency.From<RenderingPlugin>(),
    ];

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
                    static (ref FrameGraph<FrameGraph2DParam>.Builder builder, in FrameGraph2DParam param, ref PrepareTexturesPass data) =>
                    {
                        data.ColorAttachment = builder.Creates<TextureResource>(TextureDescriptor.D2(
                            FrameGraph2D.Textures.ColorTarget,
                            TextureUsage.RenderAttachment | TextureUsage.TextureBinding,
                            param.BackbufferFormat,
                            param.BackbufferSize
                        ));
                    },
                    static (context, in param, in data) =>
                    {
                        ref var commandEncoder = ref context.GetCurrentCommandEncoder();
                        using var _debugScope = commandEncoder.DebugGroupScope("Clear Color");
                        commandEncoder.InsertDebugMarker("Clear Color");

                        ref var colorTexture = ref context.Resources.GetTexture(data.ColorAttachment);

                        param.RenderAssets.Get(Blit.Handle).ClearTexture(
                            context.GPUContext, param.RenderAssets, commandEncoder,
                            colorTexture.TextureView, new Color(0.1f, 0.1f, 0.1f, 1.0f));
                    });

                frameGraph.AddPass(RenderStep2D.Main, param,
                    static (ref FrameGraph<FrameGraph2DParam>.Builder builder, in FrameGraph2DParam param, ref MainPass data) => { data.ColorAttachment = builder.Writes<TextureResource>(FrameGraph2D.Textures.ColorTarget); },
                    static (context, in param, in data) =>
                    {
                        ref var commandEncoder = ref context.GetCurrentCommandEncoder();
                        using var _debugScope = commandEncoder.DebugGroupScope("Main Pass");
                        commandEncoder.InsertDebugMarker("Main Pass");

                        using var passEncoder = commandEncoder.BeginRenderPass(new()
                        {
                            Label = "Main Pass",
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
                    static (ref FrameGraph<FrameGraph2DParam>.Builder builder, in FrameGraph2DParam param, ref UIPass data) => { data.ColorAttachment = builder.Writes<TextureResource>(FrameGraph2D.Textures.ColorTarget); },
                    static (context, in param, in data) =>
                    {
                        ref var commandEncoder = ref context.GetCurrentCommandEncoder();
                        using var _debugScope = commandEncoder.DebugGroupScope("UI Pass");
                        commandEncoder.InsertDebugMarker("UI Pass");

                        using var passEncoder = commandEncoder.BeginRenderPass(new()
                        {
                            Label = "UI Pass",
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
                        stage.Execute(passEncoder, param.RenderAssets, param.BackbufferSize);
                    });

                frameGraph.AddPass(RenderStep2D.Last, param,
                    static (ref FrameGraph<FrameGraph2DParam>.Builder builder, in FrameGraph2DParam param, ref FinalBlitPass data) =>
                    {
                        data.ColorAttachment = builder.Reads<TextureResource>(FrameGraph2D.Textures.ColorTarget);
                        data.Backbuffer = builder.Writes<TextureResource>(FrameGraph2D.Textures.Backbuffer);
                    },
                    static (context, in param, in data) =>
                    {
                        ref var commandEncoder = ref context.GetCurrentCommandEncoder();
                        using var _debugScope = commandEncoder.DebugGroupScope("Final Blit");
                        commandEncoder.InsertDebugMarker("Final Blit");

                        ref var colorTexture = ref context.Resources.GetTexture(data.ColorAttachment);
                        ref var backbufferTexture = ref context.Resources.GetTexture(data.Backbuffer);

                        param.RenderAssets.Get(Blit.Handle).BlitTexture(
                            context.GPUContext, param.RenderAssets, commandEncoder,
                            colorTexture.TextureView, backbufferTexture.TextureView, Color.BLACK);
                    });

                renderGraph.BeginFrame(ref frameGraph, ref param);
            }));

        world.Schedule.AddSystems(CoreStage.Render, FnSystem.Create(Render,
            static (FrameGraph2D renderGraph, RenderContext context) => { renderGraph.Compile().Execute(context, renderGraph.Param); }));

        world.Schedule.AddSystems(CoreStage.PostRender, FnSystem.Create(new(Cleanup)
            {
                RunsAfter = [RenderingPlugin.EndFrameSystem],
            },
            static (RenderContext context, FrameGraph2D renderGraph) =>
            {
                if (renderGraph.FrameGraph.Resources.ResourceByName.TryGetValue(FrameGraph2D.Textures.Backbuffer, out var backbufferHandle))
                {
                    context.Resources.RemoveTexture(backbufferHandle);
                }

                renderGraph.Cleanup();
            }));
    }
}