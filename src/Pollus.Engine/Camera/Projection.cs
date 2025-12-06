namespace Pollus.Engine.Camera;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.ECS;
using Pollus.Engine.Transform;
using Pollus.Mathematics;

public enum ProjectionType
{
    Orthographic,
    Perspective
}

public interface IProjection
{
    ProjectionType Type { get; }
    Mat4f GetProjection();
    void Update(Vec2<uint> size);
}

[StructLayout(LayoutKind.Explicit)]
public struct Projection : IComponent, IProjection
{
    [FieldOffset(0)]
    ProjectionType type;
    [FieldOffset(0)]
    OrthographicProjection orthographic;
    [FieldOffset(4)]
    public Vec2<uint> Size;

    public ProjectionType Type => throw new NotImplementedException();

    public Mat4f GetProjection()
    {
        return type switch
        {
            ProjectionType.Orthographic => orthographic.GetProjection(),
            _ => throw new NotImplementedException()
        };
    }

    public void Update(Vec2<uint> size)
    {
        Size = size;
        if (type == ProjectionType.Orthographic) orthographic.Update(size);
    }

    public static Projection Orthographic(OrthographicProjection projection)
    {
        return new Projection()
        {
            orthographic = projection
        };
    }

    public static Projection Orthographic(ScalingMode scalingMode, float scale, float near, float far)
    {
        return new Projection()
        {
            orthographic = new OrthographicProjection()
            {
                NearClip = near,
                FarClip = far,
                ScalingMode = scalingMode,
                Scale = scale
            }
        };
    }
}

public struct OrthographicProjection : IProjection, ComponentWrapper<OrthographicProjection>.Target<Projection>
{
    static OrthographicProjection() => ComponentWrapper<OrthographicProjection>.Target<Projection>.Init();

    public static readonly OrthographicProjection Default = new()
    {
        NearClip = 0f,
        FarClip = 100f,
        ScalingMode = ScalingMode.WindowSize(1),
        Scale = 1
    };

    readonly ProjectionType type = ProjectionType.Orthographic;
    public readonly Vec2<uint> Size;

    public float NearClip;
    public float FarClip;
    public ScalingMode ScalingMode;
    public float Scale;
    Rect area;

    public readonly Rect Area => area;
    public readonly ProjectionType Type => type;

    public OrthographicProjection() { }

    public void Update(Vec2<uint> size)
    {
        var (projWidth, projHeight) = ScalingMode switch
        {
            { Mode: ScalingMode.Type.Fixed, A: var width, B: var height } => (width, height),
            { Mode: ScalingMode.Type.WindowSize, A: var scale } => (size.X / scale, size.Y / scale),
            { Mode: ScalingMode.Type.AutoMin, A: var width, B: var height } => (uint.Max(size.X, (uint)width), uint.Max(size.Y, (uint)height)),
            { Mode: ScalingMode.Type.AutoMax, A: var width, B: var height } => (uint.Min(size.X, (uint)width), uint.Min(size.Y, (uint)height)),
            _ => throw new IndexOutOfRangeException("Unknown ScalingMode: " + nameof(ScalingMode.Mode)),
        };

        area = new Rect(0f, projHeight, projWidth, 0f);
        area.ScaleCentered(new Vec2f(Scale, Scale));
    }

    public readonly Mat4f GetProjection()
    {
        return Mat4f.OrthographicRightHanded(
            Area.Min.X, Area.Max.X, Area.Min.Y, Area.Max.Y, NearClip, FarClip
        );
    }

    public readonly Vec2f ScreenToWorld(in Transform2D cameraTransform, Vec2<int> screenPos)
    {
        var normalizedScreen = new Vec2f(
            (screenPos.X - Size.X / 2f) / (Size.X / 2f),
            -(screenPos.Y - Size.Y / 2f) / (Size.Y / 2f)
        );

        var projectionMatrix = GetProjection();
        var cameraMatrix = cameraTransform.ToMat4f();
        var viewProjectionMatrix = projectionMatrix * cameraMatrix;
        var inverseViewProjection = viewProjectionMatrix.Inverse();

        var clipSpace = new Vec4f(normalizedScreen.X, normalizedScreen.Y, 0f, 1f);
        var worldPoint = inverseViewProjection * clipSpace;
        return new Vec2f(worldPoint.X / worldPoint.W, worldPoint.Y / worldPoint.W);
    }

    public readonly Vec2<int> WorldToScreen(in Transform2D cameraTransform, Vec2f worldPos)
    {
        var cameraMatrix = cameraTransform.ToMat4f();
        var worldPoint = new Vec4f(worldPos.X, worldPos.Y, 0f, 1f);
        var transformedPoint = cameraMatrix * worldPoint;

        var normalizedScreen = new Vec2f(
            transformedPoint.X / (Area.Width / 2f),
            -transformedPoint.Y / (Area.Height / 2f)
        );

        return new Vec2<int>(
            (int)(normalizedScreen.X * (Size.X / 2f) + Size.X / 2f),
            (int)(normalizedScreen.Y * (Size.Y / 2f) + Size.Y / 2f)
        );
    }
}
