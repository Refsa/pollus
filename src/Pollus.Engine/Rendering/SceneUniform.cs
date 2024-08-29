namespace Pollus.Engine.Rendering;

using Pollus.Mathematics;

public struct SceneUniform
{
    public Mat4f View;
    public Mat4f Projection;
    public float Time;

    Vec3f _padding;
}