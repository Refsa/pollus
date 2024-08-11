namespace Pollus.Mathematics;

using System.Numerics;
using System.Runtime.CompilerServices;

public record struct Mat4<T>
    where T : struct, IFloatingPoint<T>
{
    public Vec4<T> Col0;
    public Vec4<T> Col1;
    public Vec4<T> Col2;
    public Vec4<T> Col3;

    public Mat4(Vec4<T> row0, Vec4<T> row1, Vec4<T> row2, Vec4<T> row3)
    {
        Col0 = row0;
        Col1 = row1;
        Col2 = row2;
        Col3 = row3;
    }

    public Mat4(T m00, T m01, T m02, T m03,
                T m10, T m11, T m12, T m13,
                T m20, T m21, T m22, T m23,
                T m30, T m31, T m32, T m33)
    {
        Col0 = new(m00, m01, m02, m03);
        Col1 = new(m10, m11, m12, m13);
        Col2 = new(m20, m21, m22, m23);
        Col3 = new(m30, m31, m32, m33);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4<T> OrthographicRightHanded(T left, T right, T top, T bottom, T near, T far)
    {
        var rcpWidth = T.One / (right - left);
        var rcpHeight = T.One / (top - bottom);
        var rcpDepth = T.One / (near - far);
        var two = T.CreateSaturating(2f);
        return new(
            two * rcpWidth, T.Zero, T.Zero, T.Zero,
            T.Zero, two * rcpHeight, T.Zero, T.Zero,
            T.Zero, T.Zero, rcpDepth, T.Zero,
            (left + right) * -rcpWidth, (top + bottom) * -rcpHeight, near * rcpDepth, T.One
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4<T> FromTRS(Vec3<T> translation, Quat<T> rotation, Vec3<T> scale)
    {
        var (xAxis, yAxis, zAxis) = rotation.ToAxes();
        return new(
            xAxis.Mul(scale.X),
            yAxis.Mul(scale.Y),
            zAxis.Mul(scale.Z),
            new Vec4<T>(translation, T.One)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4<T> FromTR(Vec3<T> translation, Quat<T> rotation)
    {
        var (xAxis, yAxis, zAxis) = rotation.ToAxes();
        return new(
            xAxis,
            yAxis,
            zAxis,
            new Vec4<T>(translation, T.One)
        );
    }

    public static Mat4<T> Translation(Vec3<T> translation)
    {
        return new(
            Vec4<T>.UnitX,
            Vec4<T>.UnitY,
            Vec4<T>.UnitZ,
            new Vec4<T>(translation, T.One)
        );
    }

    public static Mat4<T> Rotation(Quat<T> quat)
    {
        var (xAxis, yAxis, zAxis) = quat.ToAxes();
        return new(
            xAxis, yAxis, zAxis, Vec4<T>.UnitW
        );
    }

    public Vec3<T> GetTranslation()
    {
        return new Vec3<T>(Col3.X, Col3.Y, Col3.Z);
    }

    public Quat<T> GetRotation()
    {
        return Quat<T>.FromMat4(this);
    }
}