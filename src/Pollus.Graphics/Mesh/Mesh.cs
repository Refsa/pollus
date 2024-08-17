namespace Pollus.Graphics;

using System.Numerics;
using System.Runtime.InteropServices;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;

public enum MeshAttributeType
{
    Position = VertexFormat.Float32x3,
    Normal = VertexFormat.Float32x3,
    UV0 = VertexFormat.Float32x2,
    UV1 = VertexFormat.Float32x2,
}

public class Mesh
{
    Silk.NET.WebGPU.PrimitiveTopology primitiveTopology;
    Vec3f[] positions;
    Vec3f[]? normals;
    Vec2f[]? uv0s;
    IMeshIndices? indices;

    public Silk.NET.WebGPU.PrimitiveTopology PrimitiveTopology => primitiveTopology;

    public Mesh(
        Vec3f[] positions, Vec3f[]? normals, Vec2f[]? uv0s,
        IMeshIndices? indices,
        Silk.NET.WebGPU.PrimitiveTopology primitiveTopology = Silk.NET.WebGPU.PrimitiveTopology.TriangleList)
    {
        this.primitiveTopology = primitiveTopology;
        this.positions = positions;
        this.normals = normals;
        this.uv0s = uv0s;
        this.indices = indices;
    }

    public VertexData GetVertexData(ReadOnlySpan<MeshAttributeType> attributes)
    {
        var vertexData = VertexData.From((uint)positions.Length, MemoryMarshal.Cast<MeshAttributeType, VertexFormat>(attributes));

        for (int i = 0; i < positions.Length; i++)
        {
            for (int j = 0; j < attributes.Length; j++)
            {
                var attribute = attributes[j];
                if (attribute == MeshAttributeType.Position)
                {
                    vertexData.Write(i, positions[i], j);
                }
                else if (attribute == MeshAttributeType.Normal)
                {
                    if (normals == null) vertexData.Write(i, Vector3.Zero, j);
                    else vertexData.Write(i, normals![i], j);
                }
                else if (attribute == MeshAttributeType.UV0)
                {
                    if (uv0s == null) vertexData.Write(i, Vector2.Zero, j);
                    else vertexData.Write(i, uv0s![i], j);
                }
            }
        }

        return vertexData;
    }

    public IMeshIndices? GetIndices()
    {
        return indices;
    }

    public Span<byte> GetIndexData()
    {
        if (indices != null)
        {
            return indices.Indices;
        }

        return Span<byte>.Empty;
    }

    public int GetIndexCount()
    {
        if (indices != null)
        {
            return indices.Count;
        }
        return 0;
    }
}

public interface IMeshIndices
{
    IndexFormat Format { get; }
    Span<byte> Indices { get; }
    public int Count { get; }
}

public class MeshIndices<T> : IMeshIndices
    where T : unmanaged, IUnsignedNumber<T>, INumberBase<T>
{
    T[] indices;

    public int Count => indices.Length;
    public Span<byte> Indices => MemoryMarshal.AsBytes(indices.AsSpan());
    public IndexFormat Format => typeof(T) == typeof(uint) ? IndexFormat.Uint32 : IndexFormat.Uint16;

    public MeshIndices(T[] indices)
    {
        this.indices = indices;
    }
}