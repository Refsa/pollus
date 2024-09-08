namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class RenderingPlugin : IPlugin
{
    public const string SetupSystem = "Rendering::Setup";
    public const string UpdateSceneUniformSystem = "UpdateSceneUniform";
    public const string PrepareSceneUniformSystem = "PrepareSceneUniform";
    public const string BeginFrameSystem = "BeginFrame";
    public const string EndFrameSystem = "EndFrame";
    public const string RenderStepsCleanupSystem = "RenderStepsCleanup";
    public const string RenderingSystem = "Rendering";

    public void Apply(World world)
    {
        world.Resources.Add(new MeshRenderBatches());
        world.Resources.Init<RenderContext>();
        world.Resources.Add(new RenderSteps());
        world.Resources.Add(new RenderAssets()
            .AddLoader(new TextureRenderDataLoader())
            .AddLoader(new SamplerRenderDataLoader())
            .AddLoader(new UniformRenderDataLoader<SceneUniform>())
        );

        var assetServer = world.Resources.Get<AssetServer>();
        assetServer.AddLoader<WgslShaderSourceLoader>();

        world.AddPlugins([
            new MeshPlugin()
            {
                SharedPrimitives = PrimitiveType.All,
            },
            new ImagePlugin(),
            new CameraPlugin(),
            new MaterialPlugin<Material>(),
            new SpritePlugin(),
            new UniformPlugin<SceneUniform, Param<Time, Query<Projection, Transform2>>>()
            {
                Extract = static (in Param<Time, Query<Projection, Transform2>> param, ref SceneUniform uniform) =>
                {
                    uniform.Time = (float)param.Param0.DeltaTime;

                    if (param.Param1.EntityCount() > 0) {
                        var qCamera = param.Param1.Single();
                        uniform.Projection = qCamera.Component0.GetProjection();
                        uniform.View = qCamera.Component1.ToMat4f();
                    }
                }
            }
        ]);

        world.Schedule.AddSystems(CoreStage.Init, SystemBuilder.FnSystem(
            SetupSystem,
            static (IWGPUContext gpuContext, Resources resources) =>
            {
                resources.Add(new RenderContext
                {
                    GPUContext = gpuContext,
                });
            }
        ));

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(
            BeginFrameSystem,
            static (RenderContext context) =>
            {
                context.PrepareFrame();
                var commandEncoder = context.CreateCommandEncoder("""rendering-command-encoder""");
            }
        ));

        world.Schedule.AddSystems(CoreStage.PostRender, SystemBuilder.FnSystem(
            EndFrameSystem,
            static (RenderContext context) =>
            {
                context.EndFrame();
            }
        ));

        world.Schedule.AddSystems(CoreStage.PostRender, SystemBuilder.FnSystem(
            RenderStepsCleanupSystem,
            static (RenderSteps renderSteps) =>
            {
                renderSteps.Cleanup();
            }
        ));

        world.Schedule.AddSystems(CoreStage.Render, SystemBuilder.FnSystem(
            RenderingSystem,
            static (RenderAssets renderAssets, RenderContext context, RenderSteps renderGraph) =>
            {
                if (context.SurfaceTextureView is null) return;
                var commandEncoder = context.GetCurrentCommandEncoder();

                Span<RenderPassColorAttachment> backbuffer = stackalloc RenderPassColorAttachment[]
                {
                    new()
                    {
                        View = context.SurfaceTextureView.Value.Native,
                        LoadOp = LoadOp.Clear,
                        StoreOp = StoreOp.Store,
                        ClearValue = new(0.1f, 0.1f, 0.1f, 1.0f),
                    }
                };

                { // Clear
                    using var renderPass = commandEncoder.BeginRenderPass(new()
                    {
                        ColorAttachments = backbuffer
                    });
                }

                backbuffer[0].LoadOp = LoadOp.Load;
                for (int i = 0; i < renderGraph.Order.Count; i++)
                {
                    if (!renderGraph.Stages.TryGetValue(renderGraph.Order[i], out var stage)) continue;
                    using var renderPass = commandEncoder.BeginRenderPass(new()
                    {
                        ColorAttachments = backbuffer
                    });

                    stage.Execute(renderPass, renderAssets);
                }
            }
        ));
    }
}