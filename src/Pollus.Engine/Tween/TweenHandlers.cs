namespace Pollus.Engine.Tween;

using Pollus.Mathematics;

public interface ITweenHandler<TData>
    where TData : unmanaged
{
    TData Lerp(TData from, TData to, float t);
}

public struct FloatHandler : ITweenHandler<float>
{
    public float Lerp(float from, float to, float t) => float.Lerp(from, to, t);
}

public struct Vec2fHandler : ITweenHandler<Vec2f>
{
    public Vec2f Lerp(Vec2f from, Vec2f to, float t) => Vec2f.Lerp(from, to, t);
}