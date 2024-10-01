using System.Runtime.CompilerServices;

namespace Pollus.Mathematics.Collision2D;

public interface IShape2D
{
    Bounds2D GetAABB();
}

public static class Shape2DExt
{
    public static TShape Translate<TShape>(this TShape shape, Vec2f translation) 
        where TShape : struct, IShape2D
    {
        if (shape is Circle2D)
        {
            Unsafe.As<TShape, Circle2D>(ref shape) = Unsafe.As<TShape, Circle2D>(ref shape).Translate(translation);
            return shape;
        }
        else if (shape is Bounds2D)
        {
            Unsafe.As<TShape, Bounds2D>(ref shape) = Unsafe.As<TShape, Bounds2D>(ref shape).Translate(translation);
            return shape;
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}