namespace Pollus.Examples;

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
                frameGraph.AddTexture(TextureDescriptor.D2(
                    label: "backbuffer",
                    size: window.Size,
                    format: TextureFormat.Rgba8Unorm,
                    usage: TextureUsage.RenderAttachment
                ));

                frameGraph.AddPass("sprites-pass",
                static (ref FrameGraph<FrameGraphParam>.Builder builder, ref SpritesPassData data) =>
                {
                    data.Backbuffer = builder.Writes<TextureResource>("backbuffer");
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
                                View = context.Resources.GetTextureView(data.Backbuffer).Native,
                                LoadOp = LoadOp.Load,
                                StoreOp = StoreOp.Store,
                                ClearValue = new(0.1f, 0.1f, 0.1f, 1.0f),
                            }
                        }
                    });

                    var stage = param.RenderSteps.Stages[RenderStep2D.Main];
                    stage.Execute(passEncoder, param.RenderAssets);
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