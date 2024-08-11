using System.Numerics;
using System.Runtime.CompilerServices;

namespace Pollus.Mathematics;

public record struct Quat<T>
    where T : struct, IFloatingPoint<T>
{
    public T X;
    public T Y;
    public T Z;
    public T W;

    public Quat(T x, T y, T z, T w)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.W = w;
    }

    public Quat(Vec4<T> f) : this(f.X, f.Y, f.Z, f.W) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (Vec4<T>, Vec4<T>, Vec4<T>) ToAxes()
    {
        var x2 = X + X;
        var y2 = Y + Y;
        var z2 = Z + Z;
        var xx = X * x2;
        var xy = X * y2;
        var xz = X * z2;
        var yy = Y * y2;
        var yz = Y * z2;
        var zz = Z * z2;
        var wx = W * x2;
        var wy = W * y2;
        var wz = W * z2;

        var x_axis = new Vec4<T>(T.One - (yy + zz), xy + wz, xz - wy, T.Zero);
        var y_axis = new Vec4<T>(xy - wz, T.One - (xx + zz), yz + wx, T.Zero);
        var z_axis = new Vec4<T>(xz + wy, yz - wx, T.One - (xx + yy), T.Zero);
        return (x_axis, y_axis, z_axis);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat<T> AxisAngle(Vec3<T> axis, T angle)
    {
        (T sin, T cos) = (Math.Deg2Rad(angle) * T.CreateSaturating(0.5f)).SinCos();

        var v = axis * sin;
        return new(v.X, v.Y, v.Z, cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat<T> Identity()
    {
        return new(T.Zero, T.Zero, T.Zero, T.One);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat<T> FromMat4(Mat4<T> matrix)
    {
        return FromRotationAxes(matrix.Col0.Truncate(), matrix.Col1.Truncate(), matrix.Col2.Truncate());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat<T> FromRotationAxes(Vec3<T> x, Vec3<T> y, Vec3<T> z)
    {
        var (m00, m01, m02) = (x.X, x.Y, x.Z);
        var (m10, m11, m12) = (y.X, y.Y, y.Z);
        var (m20, m21, m22) = (z.X, z.Y, z.Z);

        if (m22 <= T.Zero)
        {
            var diff10 = m11 - m00;
            var omm22 = T.One - m22;
            if (diff10 <= T.Zero)
            {
                var fourXsq = omm22 - diff10;
                var inv4x = T.CreateChecked(0.5f) / fourXsq.Sqrt();
                return new(fourXsq * inv4x, (m01 + m10) * inv4x, (m02 + m20) * inv4x, (m12 - m21) * inv4x);
            }
            else
            {
                var fourYsq = omm22 - diff10;
                var inv4y = T.CreateChecked(0.5f) / fourYsq.Sqrt();
                return new((m01 + m10) * inv4y, fourYsq * inv4y, (m12 + m21) * inv4y, (m20 - m02) * inv4y);
            }
        }
        else
        {
            var sum10 = m11 + m00;
            var opp22 = T.One + m22;
            if (sum10 <= T.Zero)
            {
                var fourXsq = opp22 - sum10;
                var inv4x = T.CreateChecked(0.5f) / fourXsq.Sqrt();
                return new((m02 + m20) * inv4x, (m12 + m21) * inv4x, fourXsq * inv4x, (m01 - m10) * inv4x);
            }
            else
            {
                var fourYsq = opp22 + sum10;
                var inv4y = T.CreateChecked(0.5f) / fourYsq.Sqrt();
                return new((m12 - m21) * inv4y, (m20 - m02) * inv4y, (m01 - m10) * inv4y, fourYsq * inv4y);
            }
        }
    }
}