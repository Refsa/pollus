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
    public const string UpdateSceneUniformSystem = "UpdateSceneUniform";
    public const string PrepareSceneUniformSystem = "PrepareSceneUniform";
    public const string BeginFrameSystem = "BeginFrame";
    public const string EndFrameSystem = "EndFrame";
    public const string RenderStepsCleanupSystem = "RenderStepsCleanup";
    public const string RenderingSystem = "Rendering";

    public void Apply(World world)
    {
        world.Resources.Add(new MeshRenderBatches());
        world.Resources.Add(new RenderContext());
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
                    var qCamera = param.Param1.Single();

                    uniform.Time = (float)param.Param0.DeltaTime;
                    uniform.Projection = qCamera.Component0.GetProjection();
                    uniform.View = qCamera.Component1.ToMat4f();
                }
            }
        ]);

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(
            BeginFrameSystem,
            static (IWGPUContext gpuContext, RenderContext context) =>
            {
                context.Begin(gpuContext);
            }
        ));

        world.Schedule.AddSystems(CoreStage.PostRender, SystemBuilder.FnSystem(
            EndFrameSystem,
            static (IWGPUContext gpuContext, RenderContext context) =>
            {
                context.End(gpuContext);
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
                if (context.SurfaceTextureView is null || context.CommandEncoder is null) return;

                { // Clear
                    var renderPass = context.BeginRenderPass();
                    context.EndRenderPass();
                }

                for (int i = 0; i < renderGraph.Order.Count; i++)
                {
                    if (!renderGraph.Stages.TryGetValue(renderGraph.Order[i], out var stage)) continue;
                    var renderPass = context.BeginRenderPass(LoadOp.Load);

                    stage.Execute(renderPass, renderAssets);

                    context.EndRenderPass();
                }
            }
        ));
    }
}