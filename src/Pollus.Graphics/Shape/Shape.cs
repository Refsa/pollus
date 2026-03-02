namespace Pollus.Graphics;

using Core.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

[Asset]
public partial class Shape
{
    public required string Name { get; init; }
    public required Vec2f[] Positions { get; init; }
    public required Vec2f[] UVs { get; init; }

    public VertexData GetVertexData()
    {
        var vertexData = VertexData.From((uint)Positions.Length,
            stackalloc VertexFormat[] { VertexFormat.Float32x2, VertexFormat.Float32x2 });

        for (int i = 0; i < Positions.Length; i++)
        {
            vertexData.Write(i, Positions[i], UVs[i]);
        }

        return vertexData;
    }

    public static Shape Ray(Vec2f origin, Vec2f direction, float length)
    {
        return Line(origin, origin + direction * length);
    }

    public static Shape Line(Vec2f start, Vec2f end)
    {
        return new()
        {
            Name = "Line",
            Positions = [start, end],
            UVs = [Vec2f.Zero, Vec2f.One],
        };
    }

    public static Shape LineSegment(ReadOnlySpan<Vec2f> points)
    {
        var uvs = new Vec2f[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            uvs[i] = new Vec2f(i / (points.Length - 1f), 0);
        }

        return new()
        {
            Name = "LineSegment",
            Positions = points.ToArray(),
            UVs = uvs,
        };
    }

    public static Shape Rectangle(Vec2f center, Vec2f extents)
    {
        return new()
        {
            Name = "Rectangle",
            Positions = [
                center + new Vec2f(-extents.X, -extents.Y),
                center + new Vec2f(extents.X, -extents.Y),
                center + new Vec2f(-extents.X, extents.Y),
                center + new Vec2f(extents.X, extents.Y),
            ],
            UVs = [
                Vec2f.Zero,
                new Vec2f(1, 0),
                new Vec2f(0, 1),
                Vec2f.One,
            ],
        };
    }

    public static Shape Arc(Vec2f position, float radius, float angle, int resolution = 24, float angleOffset = 0f)
    {
        var positions = new Vec2f[resolution * 2];
        var uvs = new Vec2f[resolution * 2];

        angle = angle.Radians();
        var startAngle = -angle / 2 + float.Pi / 2 + angleOffset.Radians();
        var step = angle / (resolution - 1);

        for (int i = 0; i < resolution; i++)
        {
            var currAngle = startAngle + i * step;
            var cos = Math.Cos(currAngle);
            var sin = Math.Sin(currAngle);

            positions[i * 2] = position + new Vec2f(cos, sin) * radius;
            uvs[i * 2] = new Vec2f(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f);

            positions[i * 2 + 1] = position;
            uvs[i * 2 + 1] = new Vec2f(0.5f, 0.5f);
        }

        return new()
        {
            Name = "Arc",
            Positions = positions,
            UVs = uvs,
        };
    }

    public static Shape Polygon(Vec2f position, float radius, int sides = 6)
    {
        var vertexCount = (sides + 1) * 2;
        var positions = new Vec2f[vertexCount];
        var uvs = new Vec2f[vertexCount];

        for (int i = 0; i <= sides; i++)
        {
            float angle = i * float.Pi * 2f / sides;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            positions[i * 2] = position + new Vec2f(cos, sin) * radius;
            uvs[i * 2] = new Vec2f(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f);

            positions[i * 2 + 1] = position;
            uvs[i * 2 + 1] = new Vec2f(0.5f, 0.5f);
        }

        positions[sides * 2] = positions[0];
        uvs[sides * 2] = uvs[0];

        return new()
        {
            Name = "Polygon",
            Positions = positions,
            UVs = uvs,
        };
    }

    public static Shape Circle(Vec2f position, float radius, int resolution = 32)
    {
        return Polygon(position, radius, resolution);
    }

    public static Shape Triangle(Vec2f p1, Vec2f p2, Vec2f p3)
    {
        return new()
        {
            Name = "Triangle",
            Positions = [p1, p2, p3],
            UVs = [Vec2f.Zero, Vec2f.One, Vec2f.Zero],
        };
    }

    public static Shape EquilateralTriangle(Vec2f center, float size)
    {
        var height = size * Math.Sqrt(3f) * 0.5f;
        var p1 = center + new Vec2f(0, height);
        var p2 = center + new Vec2f(size, -height);
        var p3 = center + new Vec2f(-size, -height);

        return Triangle(p1, p2, p3);
    }

    public static Shape Capsule(Vec2f start, Vec2f end, float radius, int resolution = 32)
    {
        var direction = (end - start).Normalized();
        var baseAngle = Math.Atan2(direction.Y, direction.X);
        var fanCenter = (start + end) * 0.5f;

        var perimCount = resolution * 2;
        var vertexCount = (perimCount + 1) * 2;
        var positions = new Vec2f[vertexCount];
        var uvs = new Vec2f[vertexCount];

        for (int i = 0; i < resolution; i++)
        {
            float angle = baseAngle + float.Pi * 0.5f + float.Pi * i / (resolution - 1);
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            positions[i * 2] = start + new Vec2f(cos, sin) * radius;
            uvs[i * 2] = new Vec2f(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f);
            positions[i * 2 + 1] = fanCenter;
            uvs[i * 2 + 1] = new Vec2f(0.5f, 0.5f);
        }

        int offset = resolution * 2;
        for (int i = 0; i < resolution; i++)
        {
            float angle = baseAngle - float.Pi * 0.5f + float.Pi * i / (resolution - 1);
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            positions[offset + i * 2] = end + new Vec2f(cos, sin) * radius;
            uvs[offset + i * 2] = new Vec2f(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f);
            positions[offset + i * 2 + 1] = fanCenter;
            uvs[offset + i * 2 + 1] = new Vec2f(0.5f, 0.5f);
        }

        positions[^2] = positions[0];
        uvs[^2] = uvs[0];
        positions[^1] = fanCenter;
        uvs[^1] = new Vec2f(0.5f, 0.5f);

        return new()
        {
            Name = "Capsule",
            Positions = positions,
            UVs = uvs,
        };
    }

    public static Shape Kite(Vec2f center, float width, float height)
    {
        return new()
        {
            Name = "Kite",
            Positions = [
                center + new Vec2f(-width * 0.5f, -height * 0.5f),
                center + new Vec2f(width * 0.5f, -height * 0.5f),
                center + new Vec2f(0, height),
                center + new Vec2f(-width * 0.5f, -height * 0.5f),
            ],
            UVs = [
                Vec2f.Zero,
                new Vec2f(1, 0),
                new Vec2f(0.5f, 1),
                Vec2f.Zero,
            ],
        };
    }
}
