namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Mathematics;

public struct Transform2 : IComponent
{
    public static readonly Transform2 Default = new()
    {
        Position = Vec2f.Zero,
        Scale = Vec2f.One,
        Rotation = 0
    };

    public Vec2f Position;
    public Vec2f Scale;
    public float Rotation;

    public Mat4f ToMatrix()
    {
        return Mat4f.FromTRS(
            new Vec3f(Position.X, Position.Y, 0),
            Quat.AxisAngle(Vec3f.Forward, Rotation.Radians()),
            new Vec3f(Scale.X, Scale.Y, 1)
        );
    }
}