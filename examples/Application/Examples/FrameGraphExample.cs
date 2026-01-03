namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Utils;

public class FrameGraphExample : IExample
{
    public string Name => "frame-graph";

    struct SpritesPassData
    {
        public ResourceHandle<TextureResource> ColorAttachment;
    }

    struct BlitPassData
    {
        public ResourceHandle<TextureResource> ColorAttachment;
        public ResourceHandle<TextureResource> MSAAColorAttachment;
        public ResourceHandle<TextureResource> Backbuffer;
    }

    struct FrameGraphParam
    {
        public required RenderAssets RenderAssets;
        public required DrawGroups2D RenderSteps;
        public required Resources Resources;
        public required TextureFormat BackbufferFormat;
        public required Vec2<uint> BackbufferSize;
    }

    public void Run()
    {
        Application.Builder
            .AddPlugins([
                new AssetPlugin() { RootPath = "assets" },
                new RenderingPlugin(),
                new PerformanceTrackerPlugin(),
            ])
            .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
                static (Commands commands, AssetServer assetServer, Assets<SpriteMaterial> materials, Assets<SamplerAsset> samplers) =>
                {
                    var spriteMaterial = materials.Add(new SpriteMaterial
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("breakout/breakout_sheet.png"),
                        Sampler = samplers.Add(SamplerDescriptor.Nearest),
                    });

                    commands.Spawn(Camera2D.Bundle);
                    commands.Spawn(Entity.With(
                        Transform2D.Default with
                        {
                            Position = new Vec2f(100, 100),
                            Scale = new Vec2f(48, 16),
                        },
                        GlobalTransform.Default,
                        new Sprite
                        {
                            Material = spriteMaterial,
                            Slice = new Rect(16, 0, 48, 16),
                            Color = Color.WHITE,
                        }
                    ));
                }))
            .AddSystem(CoreStage.Render, FnSystem.Create("FrameGraph",
                static (RenderContext renderContext, RenderAssets renderAssets, Resources resources, DrawGroups2D renderSteps, IWindow window) =>
                {
                    using var frameGraph = new FrameGraph<FrameGraphParam>();
                    var param = new FrameGraphParam()
                    {
                        RenderAssets = renderAssets,
                        RenderSteps = renderSteps,
                        Resources = resources,
                        BackbufferFormat = renderContext.SurfaceTextureView!.Value.Descriptor.Format,
                        BackbufferSize = window.Size,
                    };

                    var backbufferDesc = TextureDescriptor.D2(
                        "backbuffer",
                        TextureUsage.RenderAttachment | TextureUsage.TextureBinding,
                        param.BackbufferFormat,
                        param.BackbufferSize
                    );
                    var backbufferHandle = frameGraph.AddTexture(new("backbuffer", backbufferDesc));
                    renderContext.Resources.SetTexture(backbufferHandle, new(null, renderContext.SurfaceTextureView!.Value, backbufferDesc));

                    frameGraph.AddPass(RenderStep2D.Main, param,
                        static (ref FrameGraph<FrameGraphParam>.Builder builder, in FrameGraphParam param, ref SpritesPassData data) =>
                        {
                            data.ColorAttachment = builder.Creates<TextureResource>(TextureDescriptor.D2(
                                "color-attachment",
                                TextureUsage.RenderAttachment | TextureUsage.TextureBinding,
                                param.BackbufferFormat,
                                param.BackbufferSize
                            ));
                        },
                        static (context, in param, in data) =>
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

                            var stage = param.RenderSteps.Groups[RenderStep2D.Main];
                            stage.Execute(passEncoder, param.RenderAssets);
                        });

                    frameGraph.AddPass(RenderStep2D.Last, param,
                        static (ref FrameGraph<FrameGraphParam>.Builder builder, in FrameGraphParam param, ref BlitPassData data) =>
                        {
                            data.ColorAttachment = builder.Reads<TextureResource>("color-attachment");
                            data.Backbuffer = builder.Writes<TextureResource>("backbuffer");
                        },
                        static (context, in param, in data) =>
                        {
                            var commandEncoder = context.GetCurrentCommandEncoder();

                            var srcTex = context.Resources.GetTexture(data.ColorAttachment);
                            var dstTex = context.Resources.GetTexture(data.Backbuffer);

                            var blit = param.RenderAssets.Get(Blit.Handle);
                            blit.BlitTexture(
                                context.GPUContext, param.RenderAssets, commandEncoder,
                                srcTex.TextureView, dstTex.TextureView,
                                clearValue: new(0.1f, 0.1f, 0.1f, 1.0f)
                            );
                        });

                    frameGraph.Compile().Execute(renderContext, param);
                }))
            .Run();
    }

    public void Stop()
    {
    }
}