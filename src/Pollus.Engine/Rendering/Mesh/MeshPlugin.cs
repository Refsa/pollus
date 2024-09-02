namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.WGPU;

public struct MeshDraw<TMaterial> : IComponent
    where TMaterial : IMaterial
{
    public required Handle<MeshAsset> Mesh;
    public required Handle<TMaterial> Material;
}

public class MeshPlugin : IPlugin
{
    static MeshPlugin()
    {
        AssetsFetch<MeshAsset>.Register();
        ResourceFetch<PrimitiveMeshes>.Register();
        ResourceFetch<MeshRenderBatches>.Register();
    }

    public PrimitiveType SharedPrimitives { get; init; }

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new MeshRenderDataLoader());

        if (SharedPrimitives != PrimitiveType.None)
        {
            var assetServer = world.Resources.Get<AssetServer>();

            var primitives = new PrimitiveMeshes();
            primitives.InitPrimitives(SharedPrimitives, assetServer.GetAssets<MeshAsset>());
            world.Resources.Add(primitives);
        }

        world.Schedule.AddSystems(CoreStage.PostInit, SystemBuilder.FnSystem(
            "SetupRendering",
            static (RenderSteps renderGraph) =>
            {
                renderGraph.Add(new MeshRenderBatchDraw());
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
    }
}