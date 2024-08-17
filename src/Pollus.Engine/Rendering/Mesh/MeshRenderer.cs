namespace Pollus.Engine.Mesh;

using Pollus.ECS;
using Pollus.Engine.Assets;

public struct MeshRenderer : IComponent
{
    public required Handle<MeshAsset> Mesh;
}
