namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;

public class MeshAsset
{
    public required string Name { get; init; }
    public required Mesh Mesh { get; init; }
}


public class MeshPlugin : IPlugin
{
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

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(
            "MeshPlugin::PrepareMeshAssets",
            static (IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets, Assets<MeshAsset> meshAssets) =>
            {
                foreach (var meshAsset in meshAssets.AssetInfos)
                {
                    renderAssets.Prepare(gpuContext, assetServer, meshAsset.Handle);
                }
            }
        ));
    }
}
