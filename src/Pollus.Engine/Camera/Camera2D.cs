namespace Pollus.Engine.Camera;

using Pollus.ECS;
using Pollus.Engine.Transform;

public struct Camera2D : IComponent
{
    public static EntityBuilder<Camera2D, Transform2, Projection> Bundle => new(
        new(),
        Transform2.Default,
        Projection.Orthographic(OrthographicProjection.Default)
    );
}
