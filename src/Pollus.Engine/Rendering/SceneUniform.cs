namespace Pollus.Engine.Rendering;

using Pollus.Mathematics;

public struct SceneUniform
{
    public Mat4f View;
    public Mat4f Projection;
    public Vec3f CameraPosition;
    public float Time;
}