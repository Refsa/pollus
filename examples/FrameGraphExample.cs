namespace Pollus.Examples;

using System.Net.Http.Headers;
using Pollus.Debugging;
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
        public ResourceHandle<TextureResource> Backbuffer;
    }

    struct FrameGraphParam
    {
        public RenderAssets RenderAssets;
        public RenderSteps RenderSteps;
        public Resources Resources;
    }

    public void Run()
    {
        Application.Builder
            .AddPlugins([
                new AssetPlugin() { RootPath = "assets" },
                new RenderingPlugin(),
                new PerformanceTrackerPlugin(),
            ])
            .AddSystem(CoreStage.PostInit, SystemBuilder.FnSystem("Setup",
            static (Commands commands, AssetServer assetServer, Assets<SpriteMaterial> materials, Assets<SamplerAsset> samplers) =>
            {
                var spriteMaterial = materials.Add(new SpriteMaterial
                {
                    ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                    Texture = assetServer.Load<ImageAsset>("breakout/breakout_sheet.png"),
                    Sampler = samplers.Add(SamplerDescriptor.Nearest),
                });

                commands.Spawn(Camera2D.Bundle);
                commands.Spawn(Entity.With(
                    Transform2.Default with
                    {
                        Position = new Vec2f(100, 100),
                        Scale = new Vec2f(48, 16),
                    },
                    new Sprite
                    {
                        Material = spriteMaterial,
                        Slice = new Rect(16, 0, 48, 16),
                        Color = Color.WHITE,
                    }
                ));
            }))
            .AddSystem(CoreStage.Render, SystemBuilder.FnSystem("FrameGraph",
            static (RenderContext renderContext, RenderAssets renderAssets, Resources resources, RenderSteps renderSteps, IWindow window) =>
            {
                using var frameGraph = new FrameGraph<FrameGraphParam>();

                var backbufferDesc = TextureDescriptor.D2(
                    "backbuffer",
                    TextureUsage.RenderAttachment | TextureUsage.TextureBinding,
                    renderContext.SurfaceTextureView!.Value.Descriptor.Format,
                    window.Size
                );
                var backbufferHandle = frameGraph.AddTexture(new("backbuffer", backbufferDesc));
                renderContext.Resources.SetTexture(backbufferHandle, new(null, renderContext.SurfaceTextureView!.Value, backbufferDesc));

                frameGraph.AddPass("sprites-pass",
                (ref FrameGraph<FrameGraphParam>.Builder builder, ref SpritesPassData data) =>
                {
                    data.ColorAttachment = builder.Creates<TextureResource>(TextureDescriptor.D2(
                        "color-attachment",
                        TextureUsage.RenderAttachment | TextureUsage.TextureBinding,
                        renderContext.SurfaceTextureView!.Value.Descriptor.Format,
                        window.Size
                    ));
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
                                ClearValue = new(0.1f, 0.1f, 0.1f, 1.0f),
                            }
                        }
                    });

                    var stage = param.RenderSteps.Stages[RenderStep2D.Main];
                    stage.Execute(passEncoder, param.RenderAssets);
                });

                frameGraph.AddPass("blit-pass",
                static (ref FrameGraph<FrameGraphParam>.Builder builder, ref BlitPassData data) =>
                {
                    data.ColorAttachment = builder.Reads<TextureResource>("color-attachment");
                    data.Backbuffer = builder.Writes<TextureResource>("backbuffer");
                },
                static (context, param, data) =>
                {
                    var commandEncoder = context.GetCurrentCommandEncoder();

                    var srcTex = context.Resources.GetTexture(data.ColorAttachment);
                    var dstTex = context.Resources.GetTexture(data.Backbuffer);

                    var blit = param.RenderAssets.Get<Blit>(Blit.Handle);
                    blit.BlitTexture(context.GPUContext, commandEncoder, srcTex.TextureView, dstTex.TextureView);
                });

                frameGraph.Compile().Execute(renderContext, new()
                {
                    RenderAssets = renderAssets,
                    RenderSteps = renderSteps,
                    Resources = resources,
                });
            }))
            .Run();
    }

    public void Stop()
    {

    }
}