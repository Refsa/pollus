namespace Pollus.Engine.Rendering;

using ImGuiNET;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class RenderingPlugin : IPlugin
{
    static RenderingPlugin()
    {
        AssetsFetch<MeshAsset>.Register();
        AssetsFetch<ImageAsset>.Register();
        AssetsFetch<SamplerAsset>.Register();
        AssetsFetch<ShaderAsset>.Register();
        AssetsFetch<UniformAsset<SceneUniform>>.Register();
        ResourceFetch<RenderAssets>.Register();
        ResourceFetch<RenderBatches>.Register();
        ResourceFetch<RenderContext>.Register();
        ResourceFetch<RenderSteps>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Add(new RenderBatches());
        world.Resources.Add(new RenderContext());
        world.Resources.Add(new RenderSteps());
        world.Resources.Add(new RenderAssets()
            .AddLoader(new MeshRenderDataLoader())
            .AddLoader(new TextureRenderDataLoader())
            .AddLoader(new SamplerRenderDataLoader())
            .AddLoader(new UniformRenderDataLoader<SceneUniform>())
        );

        var assetServer = world.Resources.Get<AssetServer>();
        assetServer.AddLoader<WgslShaderSourceLoader>();
        assetServer.GetAssets<UniformAsset<SceneUniform>>().Add(new UniformAsset<SceneUniform>(new()));

        world.AddPlugins([
            new MeshPlugin()
            {
                SharedPrimitives = PrimitiveType.All,
            },
            new ImagePlugin(),
            new CameraPlugin(),
            new MaterialPlugin<Material>(),
            new SpritePlugin(),
        ]);

        world.Schedule.AddSystems(CoreStage.PostInit, SystemBuilder.FnSystem(
            "SetupRendering",
            static (RenderSteps renderGraph) => 
            {
                renderGraph.Add(new RenderBatchDraw());
                renderGraph.Add(new SpriteBatchDraw());
            }
        ));

        world.Schedule.AddSystems(CoreStage.Last, SystemBuilder.FnSystem(
            "UpdateSceneUniform",
            static (Assets<UniformAsset<SceneUniform>> uniformAssets, Time time, Query<Projection, Transform2>.Filter<All<Camera2D>> qCamera) =>
            {
                var handle = new Handle<UniformAsset<SceneUniform>>(0);
                var uniformAsset = uniformAssets.Get(handle)!;

                var sceneUniform = uniformAsset.Value;
                sceneUniform.Time = (float)time.SecondsSinceStartup;
                qCamera.ForEach((ref Projection projection, ref Transform2 transform) =>
                {
                    sceneUniform.Projection = projection.GetProjection();
                    sceneUniform.View = transform.ToMat4f();
                });

                uniformAsset.Value = sceneUniform;
            }
        ));

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(
            "PrepareSceneUniform",
            static (IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets, Assets<UniformAsset<SceneUniform>> uniformAssets) =>
            {
                var handle = new Handle<UniformAsset<SceneUniform>>(0);
                var uniformAsset = uniformAssets.Get(handle)!;
                renderAssets.Prepare(gpuContext, assetServer, handle);
                renderAssets.Get<UniformRenderData>(handle).WriteBuffer(uniformAsset.Value);
            }
        ));

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(
            "PrepareMeshAssets",
            static (IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets) =>
            {
                foreach (var handle in assetServer.GetAssets<MeshAsset>().Handles)
                {
                    renderAssets.Prepare(gpuContext, assetServer, handle);
                }
            }
        ));

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(
            "BeginFrame",
            static (IWGPUContext gpuContext, RenderContext context) =>
            {
                context.Begin(gpuContext);
            }
        ));

        world.Schedule.AddSystems(CoreStage.PostRender, SystemBuilder.FnSystem(
            "EndFrame",
            static (IWGPUContext gpuContext, RenderContext context) =>
            {
                context.End(gpuContext);
            }
        ));

        world.Schedule.AddSystems(CoreStage.Render, SystemBuilder.FnSystem(
            "Rendering",
            static (RenderAssets renderAssets, Resources resources, RenderContext context, RenderSteps renderGraph) =>
            {
                if (context.SurfaceTextureView is null || context.CommandEncoder is null) return;

                { // Clear
                    var renderPass = context.BeginRenderPass();
                    context.EndRenderPass();
                }

                foreach (var stage in renderGraph.Stages.OrderBy(e => e.Key))
                {
                    var renderPass = context.BeginRenderPass(LoadOp.Load);

                    foreach (var draw in stage.Value)
                    {
                        draw.Render(renderPass, resources, renderAssets);
                    }

                    context.EndRenderPass();
                }
            }
        ));
    }
}