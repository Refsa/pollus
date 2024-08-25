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

                var camera = qCamera.Single();
                sceneUniform.Projection = camera.Component0.GetProjection();
                sceneUniform.View = camera.Component1.ToMat4f();

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
                foreach (var meshAsset in assetServer.GetAssets<MeshAsset>().AssetInfos)
                {
                    renderAssets.Prepare(gpuContext, assetServer, meshAsset.Handle);
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

                for (int i = 0; i < renderGraph.Order.Count; i++)
                {
                    if (!renderGraph.Stages.TryGetValue(renderGraph.Order[i], out var stage)) continue;
                    if (stage.Count == 0) continue;

                    var renderPass = context.BeginRenderPass(LoadOp.Load);

                    foreach (var draw in stage)
                    {
                        draw.Render(renderPass, resources, renderAssets);
                    }

                    context.EndRenderPass();
                }
            }
        ));
    }
}