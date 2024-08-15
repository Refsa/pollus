namespace Pollus.Engine.Camera;

public struct ScalingMode
{
    public enum Type
    {
        WindowSize,
        Fixed,
        AutoMin,
        AutoMax,
    }

    Type mode;
    int a;
    int b;

    public Type Mode => mode;
    public int A => a;
    public int B => b;

    public static ScalingMode WindowSize(int scale)
    {
        return new ScalingMode
        {
            mode = Type.WindowSize,
            a = scale,
        };
    }

    public static ScalingMode Fixed(int width, int height)
    {
        return new ScalingMode
        {
            mode = Type.Fixed,
            a = width,
            b = height
        };
    }

    public static ScalingMode AutoMin(int width, int height)
    {
        return new ScalingMode
        {
            mode = Type.AutoMin,
            a = width,
            b = height
        };
    }

    public static ScalingMode AutoMax(int width, int height)
    {
        return new ScalingMode
        {
            mode = Type.AutoMax,
            a = width,
            b = height
        };
    }
}