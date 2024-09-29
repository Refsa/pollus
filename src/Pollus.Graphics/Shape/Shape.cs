namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public class Shape
{
    public required string Name { get; init; }
    public required Vec2f[] Positions { get; init; }
    public required Vec2f[] Uvs { get; init; }

    public VertexData GetVertexData()
    {
        var vertexData = VertexData.From((uint)Positions.Length,
            stackalloc VertexFormat[] { VertexFormat.Float32x2, VertexFormat.Float32x2 });

        for (int i = 0; i < Positions.Length; i++)
        {
            vertexData.Write(i, Positions[i], Uvs[i]);
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
            Uvs = [Vec2f.Zero, Vec2f.One],
        };
    }

    public static Shape LineSegment(ReadOnlySpan<Vec2f> points)
    {
        var uvs = new Vec2f[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            uvs[i] = new Vec2f(i / (points.Length - 1), 0);
        }

        return new()
        {
            Name = "LineSegment",
            Positions = points.ToArray(),
            Uvs = uvs,
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
                center + new Vec2f(extents.X, extents.Y),
                center + new Vec2f(-extents.X, extents.Y),
                center + new Vec2f(-extents.X, -extents.Y),
            ],
            Uvs = [
                Vec2f.Zero,
                new Vec2f(1, 0),
                Vec2f.One,
                new Vec2f(0, 1),
                Vec2f.Zero,
            ],
        };
    }

    public static Shape Arc(Vec2f position, float radius, float angle, int resolution = 24, float angleOffset = 0f)
    {
        var positions = new Vec2f[resolution * 2];
        var uvs = new Vec2f[resolution * 2];

        angle = angle.Radians();
        var currAngle = -angle / 2 + float.Pi / 2 + angleOffset.Radians();
        var step = angle / (resolution - 1);

        for (int i = 0; i < resolution * 2 - 1; i += 2)
        {
            positions[i] = position + new Vec2f(Math.Cos(currAngle), Math.Sin(currAngle)) * radius;
            uvs[i] = new Vec2f(Math.Cos(currAngle) * 0.5f + 0.5f, Math.Sin(currAngle) * 0.5f + 0.5f);
            currAngle += step;

            positions[i + 1] = position;
            uvs[i + 1] = new Vec2f(0.5f, 0.5f);
        }

        positions[^1] = positions[0];
        uvs[^1] = uvs[0];

        return new()
        {
            Name = "Arc",
            Positions = positions,
            Uvs = uvs,
        };
    }

    public static Shape Polygon(Vec2f position, float radius, int sides = 6)
    {
        sides += 1;
        var positions = new Vec2f[sides * 2];
        var uvs = new Vec2f[sides * 2];

        for (int i = 0; i < sides * 2 - 1; i += 2)
        {
            float angle = i / 2 * float.Pi * 2f / (sides - 1);
            positions[i] = position + new Vec2f(Math.Cos(angle), Math.Sin(angle)) * radius;
            uvs[i] = new Vec2f(Math.Cos(angle) * 0.5f + 0.5f, Math.Sin(angle) * 0.5f + 0.5f);

            positions[i + 1] = position;
            uvs[i + 1] = new Vec2f(0.5f, 0.5f);
        }

        positions[^1] = positions[0];
        uvs[^1] = uvs[0];

        return new()
        {
            Name = "Circle",
            Positions = positions,
            Uvs = uvs,
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
            Uvs = [Vec2f.Zero, Vec2f.One, Vec2f.Zero],
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
        var perpendicular = new Vec2f(-direction.Y, direction.X);
        var angleOffset = (Math.Atan2(direction.Y, direction.X) + float.Pi * 0.5f).Degrees();

        var arc1 = Arc(start, radius, 180f, resolution, angleOffset);
        var arc2 = Arc(end, radius, 180f, resolution, 180f + angleOffset);
        var line1 = Line(start - perpendicular * radius, end + perpendicular * radius);
        var line2 = Line(start - perpendicular * radius, end + perpendicular * radius);

        return new()
        {
            Name = "Capsule",
            Positions = [.. arc1.Positions, .. line1.Positions, .. line2.Positions, .. arc2.Positions],
            Uvs = [.. arc1.Uvs, .. line1.Uvs, .. line2.Uvs, .. arc2.Uvs],
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
            Uvs = [
                Vec2f.Zero,
                new Vec2f(1, 0),
                new Vec2f(0.5f, 1),
                Vec2f.Zero,
            ],
        };
    }
}
