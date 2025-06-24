namespace Pollus.Mathematics;

public static class VecExt
{
    public static Vec2f ToVec2f(this in Vec2<int> vec2i)
    {
        return new Vec2f(vec2i.X, vec2i.Y);
    }

    public static Vec2f ToVec2f(this in Vec2<float> vec2f)
    {
        return new Vec2f(vec2f.X, vec2f.Y);
    }
}
