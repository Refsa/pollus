namespace Pollus.Engine.Rendering;

using Pollus.Assets;
using Pollus.Graphics;
using Pollus.Utils;

[Flags]
public enum PrimitiveType
{
    None = 0,
    Quad = 1 << 0,
    All = ~0,
}

public class PrimitiveMeshes
{
    public Handle<MeshAsset> Quad { get; private set; }

    public Handle<MeshAsset> CreatePrimitive(PrimitiveType type, bool unique, Assets<MeshAsset> assets)
    {
        return type switch
        {
            PrimitiveType.Quad => assets.Add(new MeshAsset { Name = "Quad", Mesh = unique ? Primitives.CreateQuad() : Primitives.SharedQuad }, null),
            _ => throw new NotImplementedException("CreatePrimitive requires a single primitive type"),
        };
    }

    public void InitPrimitives(PrimitiveType types, Assets<MeshAsset> assets)
    {
        if (types.HasFlag(PrimitiveType.Quad))
        {
            Quad = CreatePrimitive(PrimitiveType.Quad, true, assets);
        }
    }
}