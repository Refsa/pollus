namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;

public struct Renderable<TMaterial> : IComponent
    where TMaterial : IMaterial
{
    public required Handle<MeshAsset> Mesh;
    public required Handle<TMaterial> Material;
}
