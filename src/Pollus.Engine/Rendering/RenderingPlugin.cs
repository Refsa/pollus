namespace Pollus.Engine.Rendering;

using System.Runtime.InteropServices;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Debug;
using Pollus.Mathematics;
using Silk.NET.SDL;

public class RenderingPlugin : IPlugin
{
    static RenderingPlugin()
    {
        AssetsFetch<Material>.Register();
        AssetsFetch<MeshAsset>.Register();
        AssetsFetch<ImageAsset>.Register();
        AssetsFetch<SamplerAsset>.Register();
        AssetsFetch<ShaderAsset>.Register();
        AssetsFetch<UniformAsset<SceneUniform>>.Register();
        ResourceFetch<RenderAssets>.Register();
        ResourceFetch<RenderableBatches>.Register();
        ResourceFetch<RenderContext>.Register();
    }

    public void Apply(World world)
    {
        world.AddPlugins([
            new MeshPlugin()
            {
                SharedPrimitives = PrimitiveType.All,
            },
            new ImagePlugin(),
            new CameraPlugin(),
        ]);

        var assetServer = world.Resources.Get<AssetServer>();
        assetServer.AddLoader<WgslShaderSourceLoader>();
        assetServer.GetAssets<UniformAsset<SceneUniform>>().Add(new UniformAsset<SceneUniform>(new()));

        world.Resources.Add(new RenderableBatches());
        world.Resources.Add(new RenderContext());
        world.Resources.Add(new RenderAssets()
            .AddLoader(new MeshRenderDataLoader())
            .AddLoader(new TextureRenderDataLoader())
            .AddLoader(new SamplerRenderDataLoader())
            .AddLoader(new UniformRenderDataLoader<SceneUniform>())
            .AddLoader(new MaterialRenderDataLoader<Material>())
        );

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(
            "PrepareRenderable",
            static (RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, RenderableBatches batches, Query<Transform2, Renderable> query) =>
            {
                query.ForEach((ref Transform2 transform, ref Renderable renderable) =>
                {
                    if (!batches.TryGetBatch(renderable.Mesh, renderable.Material, out var batch))
                    {
                        batch = batches.CreateBatch(gpuContext, 32, renderable.Mesh, renderable.Material);
                    }
                    if (batch.IsFull)
                    {
                        batch.Resize(gpuContext, batch.Capacity * 2);
                    }
                    batch.Write(transform.ToMatrix());

                    renderAssets.Prepare(gpuContext, assetServer, renderable.Mesh);
                    renderAssets.Prepare(gpuContext, assetServer, renderable.Material);
                });
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
                    sceneUniform.View = transform.ToMatrix().Inverse();
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
            "RenderRenderable",
            static (RenderAssets renderAssets, IWGPUContext gpuContext, RenderableBatches batches, RenderContext context) =>
            {
                if (context.SurfaceTextureView is null || context.CommandEncoder is null) return;
                var commandEncoder = context.CommandEncoder.Value;
                var surfaceTextureView = context.SurfaceTextureView.Value;

                foreach (var batch in batches.Batches)
                {
                    batch.WriteBuffer();

                    var material = renderAssets.Get<MaterialRenderData>(batch.Material);
                    var mesh = renderAssets.Get<MeshRenderData>(batch.Mesh);

                    using var renderPass = commandEncoder.BeginRenderPass(new()
                    {
                        Label = """RenderPass""",
                        ColorAttachments = new[]
                        {
                            new RenderPassColorAttachment()
                            {
                                View = surfaceTextureView.Native,
                                LoadOp = LoadOp.Clear,
                                StoreOp = StoreOp.Store,
                                ClearValue = new(0.15f, 0.125f, 0.1f, 1.0f),
                            },
                        },
                    });

                    renderPass.SetPipeline(material.Pipeline);
                    for (int i = 0; i < material.BindGroups.Length; i++)
                    {
                        renderPass.SetBindGroup(material.BindGroups[i], (uint)i);
                    }

                    if (mesh.IndexBuffer != null)
                    {
                        renderPass.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
                    }

                    renderPass.SetVertexBuffer(0, mesh.VertexBuffer);
                    renderPass.SetVertexBuffer(1, batch.InstanceBuffer);
                    renderPass.DrawIndexed((uint)mesh.IndexCount, (uint)batch.Count, 0, 0, 0);

                    renderPass.End();

                    batch.Reset();
                }
            }
        ));
    }
}

public struct Renderable : IComponent
{
    public required Handle<MeshAsset> Mesh;
    public required Handle Material;
}