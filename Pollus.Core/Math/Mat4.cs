namespace Pollus.Mathematics;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

public record struct Mat4<T>
    where T : struct, IFloatingPoint<T>
{
    public static int SizeInBytes => Unsafe.SizeOf<T>() * 16;

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
    public static Mat4<T> Identity()
    {
        return new(
            T.One, T.Zero, T.Zero, T.Zero,
            T.Zero, T.One, T.Zero, T.Zero,
            T.Zero, T.Zero, T.One, T.Zero,
            T.Zero, T.Zero, T.Zero, T.One
        );
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

    public void Translate(Vec3<T> translation)
    {
        Col3 = Col3.Add(new Vec4<T>(translation, T.Zero));
    }

    public Vec3<T> GetTranslation()
    {
        return new Vec3<T>(Col3.X, Col3.Y, Col3.Z);
    }

    public Quat<T> GetRotation()
    {
        return Quat<T>.FromMat4(this);
    }

    public Mat4<T> Inverse()
    {
        var (m00, m01, m02, m03) = (Col0.X, Col0.Y, Col0.Z, Col0.W);
        var (m10, m11, m12, m13) = (Col1.X, Col1.Y, Col1.Z, Col1.W);
        var (m20, m21, m22, m23) = (Col2.X, Col2.Y, Col2.Z, Col2.W);
        var (m30, m31, m32, m33) = (Col3.X, Col3.Y, Col3.Z, Col3.W);

        var coef00 = m22 * m33 - m32 * m23;
        var coef02 = m12 * m33 - m32 * m13;
        var coef03 = m12 * m23 - m22 * m13;

        var coef04 = m21 * m33 - m31 * m23;
        var coef06 = m11 * m33 - m31 * m13;
        var coef07 = m11 * m23 - m21 * m13;

        var coef08 = m21 * m32 - m31 * m22;
        var coef10 = m11 * m32 - m31 * m12;
        var coef11 = m11 * m22 - m21 * m12;

        var coef12 = m20 * m33 - m30 * m23;
        var coef14 = m10 * m33 - m30 * m13;
        var coef15 = m10 * m23 - m20 * m13;

        var coef16 = m20 * m32 - m30 * m22;
        var coef18 = m10 * m32 - m30 * m12;
        var coef19 = m10 * m22 - m20 * m12;

        var coef20 = m20 * m31 - m30 * m21;
        var coef22 = m10 * m31 - m30 * m11;
        var coef23 = m10 * m21 - m20 * m11;

        var fac0 = new Vec4<T>(coef00, coef00, coef02, coef03);
        var fac1 = new Vec4<T>(coef04, coef04, coef06, coef07);
        var fac2 = new Vec4<T>(coef08, coef08, coef10, coef11);
        var fac3 = new Vec4<T>(coef12, coef12, coef14, coef15);
        var fac4 = new Vec4<T>(coef16, coef16, coef18, coef19);
        var fac5 = new Vec4<T>(coef20, coef20, coef22, coef23);

        var vec0 = new Vec4<T>(m10, m00, m00, m00);
        var vec1 = new Vec4<T>(m11, m01, m01, m01);
        var vec2 = new Vec4<T>(m12, m02, m02, m02);
        var vec3 = new Vec4<T>(m13, m03, m03, m03);

        var inv0 = vec1.Mul(fac0).Sub(vec2.Mul(fac1)).Add(vec3.Mul(fac2));
        var inv1 = vec0.Mul(fac0).Sub(vec2.Mul(fac3)).Add(vec3.Mul(fac4));
        var inv2 = vec0.Mul(fac1).Sub(vec1.Mul(fac3)).Add(vec3.Mul(fac5));
        var inv3 = vec0.Mul(fac2).Sub(vec1.Mul(fac4)).Add(vec2.Mul(fac5));

        var sign_a = new Vec4<T>(T.One, -T.One, T.One, -T.One);
        var sign_b = new Vec4<T>(-T.One, T.One, -T.One, T.One);

        var inverse = new Mat4<T>(
            inv0.Mul(sign_a),
            inv1.Mul(sign_b),
            inv2.Mul(sign_a),
            inv3.Mul(sign_b)
        );

        var col0 = new Vec4<T>(
            inverse.Col0.X,
            inverse.Col1.X,
            inverse.Col2.X,
            inverse.Col3.X
        );

        var dot0 = Col0.Mul(col0);
        var dot1 = dot0.X + dot0.Y + dot0.Z + dot0.W;

        var rcp_det = dot1.Rcp();
        return inverse.Mul(rcp_det);
    }

    public Mat4<T> Mul(T other)
    {
        return new(
            Col0.Mul(other),
            Col1.Mul(other),
            Col2.Mul(other),
            Col3.Mul(other)
        );
    }
}