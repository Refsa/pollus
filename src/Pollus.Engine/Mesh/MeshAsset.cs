namespace Pollus.Engine.Mesh;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics;

public struct MeshRenderer : IComponent
{
    public Handle<MeshAsset> Mesh;
}

public class MeshAsset
{
    public required Mesh Mesh { get; init; }
}

public class MeshPlugin : IPlugin
{
    static MeshPlugin()
    {
        AssetsFetch<MeshAsset>.Register();
        ResourceFetch<PrimitiveMeshes>.Register();
    }

    public PrimitiveType SharedPrimitives { get; init; }

    public void Apply(World world)
    {
        if (SharedPrimitives != PrimitiveType.None)
        {
            var assetServer = world.Resources.Get<AssetServer>();
            
            var primitives = new PrimitiveMeshes();
            primitives.InitPrimitives(SharedPrimitives, assetServer.GetAssets<MeshAsset>());
            world.Resources.Add(primitives);
        }
    }
}