namespace Pollus.Engine.Rendering;

using Pollus.Engine.Window;
using Pollus.Debugging;
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
    public const string BeginFrameSystem = "Rendering::BeginFrame";
    public const string EndFrameSystem = "Rendering::EndFrame";
    public const string RenderStepsCleanupSystem = "Rendering::RenderStepsCleanup";
    public const string WindowResizedSystem = "Rendering::WindowResized";

    public void Apply(World world)
    {
        world.Resources.Init<RenderContext>();
        world.Resources.Add(new MeshRenderBatches());
        world.Resources.Add(new DrawGroups2D());
        world.Resources.Add(new RenderAssets()
            .AddLoader(new TextureRenderDataLoader<Texture2D>())
            .AddLoader(new SamplerRenderDataLoader())
            .AddLoader(new StorageBufferRenderDataLoader())
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
            new ComputePlugin<ComputeShader>(),
            new SpritePlugin(),
            new FrameGraph2DPlugin(),
            new UniformPlugin<SceneUniform, Param<Time, Query<Projection, Transform2D>>>()
            {
                Extract = static (in param, ref uniform) =>
                {
                    var (time, qCamera) = param;

                    uniform.Time = (float)time.DeltaTime;
                    if (qCamera.EntityCount() == 0)
                    {
                        return;
                    }

                    var camera = qCamera.Single();
                    uniform.Projection = camera.Component0.GetProjection();
                    uniform.View = camera.Component1.ToMat4f();
                }
            }
        ]);

        world.Schedule.AddSystems(CoreStage.Init, FnSystem.Create(
            SetupSystem,
            static (IWGPUContext gpuContext, Resources resources, RenderAssets renderAssets) =>
            {
                resources.Add(new RenderContext
                {
                    GPUContext = gpuContext,
                });

                renderAssets.Add(Blit.Handle, new Blit());
            }
        ));

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(
            BeginFrameSystem,
            static (RenderContext context) => { context.PrepareFrame(); }
        ));

        world.Schedule.AddSystems(CoreStage.PostRender, FnSystem.Create(
            EndFrameSystem,
            static (RenderContext context) => { context.EndFrame(); }
        ));

        world.Schedule.AddSystems(CoreStage.PostRender, FnSystem.Create(
            new(RenderStepsCleanupSystem)
            {
                RunsAfter = [EndFrameSystem],
            },
            static (RenderContext context, DrawGroups2D renderSteps, FrameGraph2D frameGraph) =>
            {
                context.CleanupFrame();
                frameGraph.Cleanup();
                renderSteps.Cleanup();
            }
        ));

        world.Schedule.AddSystems(CoreStage.PostRender, FnSystem.Create(new(WindowResizedSystem)
            {
                RunsAfter = [RenderStepsCleanupSystem],
            },
            static (EventReader<WindowEvent.Resized> eWindowResized, IWGPUContext gpuContext) =>
            {
                if (eWindowResized.HasAny)
                {
                    var events = eWindowResized.Read();
                    gpuContext.ResizeSurface(events[^1].Size);
                }
            }));
    }
}