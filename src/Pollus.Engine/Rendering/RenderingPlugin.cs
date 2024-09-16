namespace Pollus.Engine.Rendering;

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
    public const string UpdateSceneUniformSystem = "Rendering::UpdateSceneUniform";
    public const string PrepareSceneUniformSystem = "Rendering::PrepareSceneUniform";
    public const string BeginFrameSystem = "Rendering::BeginFrame";
    public const string EndFrameSystem = "Rendering::EndFrame";
    public const string RenderStepsCleanupSystem = "Rendering::RenderStepsCleanup";

    public void Apply(World world)
    {
        world.Resources.Add(new MeshRenderBatches());
        world.Resources.Init<RenderContext>();
        world.Resources.Add(new DrawGroups2D());
        world.Resources.Add(new RenderAssets()
            .AddLoader(new TextureRenderDataLoader<Texture2D>())
            .AddLoader(new SamplerRenderDataLoader())
            .AddLoader(new StorageBufferRenderDataLoader())
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
            new FrameGraph2DPlugin(),
            new UniformPlugin<SceneUniform, Param<Time, Query<Projection, Transform2>>>()
            {
                Extract = static (in Param<Time, Query<Projection, Transform2>> param, ref SceneUniform uniform) =>
                {
                    uniform.Time = (float)param.Param0.DeltaTime;
                    Guard.IsTrue(param.Param1.EntityCount() > 0, "No camera entity found");

                    var qCamera = param.Param1.Single();
                    uniform.Projection = qCamera.Component0.GetProjection();
                    uniform.View = qCamera.Component1.ToMat4f();
                }
            }
        ]);

        world.Schedule.AddSystems(CoreStage.Init, SystemBuilder.FnSystem(
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

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(
            BeginFrameSystem,
            static (RenderContext context) =>
            {
                context.PrepareFrame();
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
            static (RenderContext context, DrawGroups2D renderSteps) =>
            {
                context.CleanupFrame();
                renderSteps.Cleanup();
            }
        ).After(EndFrameSystem));
    }
}