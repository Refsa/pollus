namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Engine.Window;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public class RenderingPlugin : IPlugin
{
    public const string SetupSystem = "Rendering::Setup";
    public const string BeginFrameSystem = "Rendering::BeginFrame";
    public const string EndFrameSystem = "Rendering::EndFrame";
    public const string RenderStepsCleanupSystem = "Rendering::RenderStepsCleanup";
    public const string WindowResizedSystem = "Rendering::WindowResized";

    static RenderingPlugin()
    {
        ResourceFetch<IWGPUContext>.Register();
        ResourceFetch<GraphicsContext>.Register();
        ResourceFetch<RenderContext>.Register();
        ResourceFetch<DrawGroups2D>.Register();
        ResourceFetch<RenderAssets>.Register();
    }

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<WindowPlugin>(),
        PluginDependency.From(() => AssetPlugin.Default),
    ];

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
        assetServer.InitAsset<Texture2D>();
        assetServer.InitAsset<SamplerAsset>();
        assetServer.InitAsset<StorageBuffer>();

        world.AddPlugins([
            new ImagePlugin(),
            new CameraPlugin(),
            new FrameGraph2DPlugin(),
            new MeshPlugin() { SharedPrimitives = PrimitiveType.All },
            new ComputePlugin<ComputeShader>(),
            new SpritePlugin(),
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
            },
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
            static (RenderContext context, IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets, EventReader<AssetEvent<Texture2D>> assetEvents) =>
            {
                foreach (scoped ref readonly var assetEvent in assetEvents.Read())
                {
                    if (assetEvent.Type is AssetEventType.Unloaded) continue;
                    renderAssets.Prepare(gpuContext, assetServer, assetEvent.Handle, assetEvent.Type is AssetEventType.Changed);
                }

                context.PrepareFrame();
            }
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
            static (RenderContext context, DrawGroups2D renderSteps) =>
            {
                context.CleanupFrame();
                renderSteps.Cleanup();
            }
        ));

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(new(WindowResizedSystem)
            {
                RunsAfter = [RenderStepsCleanupSystem],
            },
            static (EventReader<WindowEvent.Resized> eWindowResized, RenderContext renderContext, RenderAssets renderAssets, IWGPUContext gpuContext) =>
            {
                if (eWindowResized.HasAny)
                {
                    var events = eWindowResized.Read();

                    renderContext.Resources.Cleanup();
                    if (renderAssets.TryGet(Blit.Handle, out Blit blit))
                    {
                        blit.CleanupBindGroups(renderAssets);
                    }

                    gpuContext.ResizeSurface(events[^1].Size);
                }
            }));
    }
}