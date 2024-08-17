namespace Pollus.Graphics;

using Pollus.Mathematics;

public static class Primitives
{
    static Mesh? sharedQuad;

    public static Mesh SharedQuad => sharedQuad ??= CreateQuad();

    public static Mesh CreateQuad()
    {
        return new Mesh(
            [
                new Vec3f(-0.5f, -0.5f, 0.0f),
                new Vec3f(+0.5f, -0.5f, 0.0f),
                new Vec3f(+0.5f, +0.5f, 0.0f),
                new Vec3f(-0.5f, +0.5f, 0.0f),
            ],
            [
                new Vec3f(0.0f, 0.0f, 1.0f),
                new Vec3f(0.0f, 0.0f, 1.0f),
                new Vec3f(0.0f, 0.0f, 1.0f),
                new Vec3f(0.0f, 0.0f, 1.0f),
            ],
            [
                new Vec2f(0.0f, 0.0f),
                new Vec2f(1.0f, 0.0f),
                new Vec2f(1.0f, 1.0f),
                new Vec2f(0.0f, 1.0f),
            ],
            new MeshIndices<ushort>([
                0, 1, 2,
                2, 3, 0,
            ])
        );
    }
}
