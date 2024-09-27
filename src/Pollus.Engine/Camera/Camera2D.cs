namespace Pollus.Engine.Camera;

using Pollus.ECS;
using Pollus.Engine.Transform;
using Pollus.Mathematics;

public struct Camera2D : IComponent
{
    public static EntityBuilder<Camera2D, Transform2D, Projection> Bundle => new(
        new(),
        Transform2D.Default,
        Projection.Orthographic(OrthographicProjection.Default)
    );
}
