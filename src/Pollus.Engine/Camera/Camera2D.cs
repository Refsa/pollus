namespace Pollus.Engine.Camera;

using Pollus.ECS;
using Pollus.Engine.Transform;

[Required<Transform2D>, Required<Projection>(nameof(DefaultOrthoProjection))]
public partial struct Camera2D : IComponent
{
    public static EntityBuilder<Camera2D, Transform2D, Projection> Bundle => new(
        new(),
        Transform2D.Default,
        Projection.Orthographic(OrthographicProjection.Default)
    );

    public static Projection DefaultOrthoProjection() => Projection.Orthographic(OrthographicProjection.Default);
}
