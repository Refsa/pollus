namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Mathematics;

public struct GlobalTransform : IComponent
{
    public static readonly GlobalTransform Default = new()
    {
        Value = Mat4f.Identity(),
    };

    public Mat4f Value;

    public Transform2D ToTransform2D()
    {
        return new Transform2D
        {
            Position = Value.GetTranslation().XY,
            Rotation = Value.GetRotationZ(),
            Scale = Value.GetScale().XY,
        };
    }

    public Transform3D ToTransform3D()
    {
        return new Transform3D
        {
            Position = Value.GetTranslation(),
            Rotation = Value.GetRotation(),
            Scale = Value.GetScale(),
        };
    }
}