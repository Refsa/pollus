using System.Numerics;
using System.Runtime.CompilerServices;

namespace Pollus.Mathematics;

public record struct Quat
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Quat(float x, float y, float z, float w)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.W = w;
    }

    public Quat(Vec4f f) : this(f.X, f.Y, f.Z, f.W) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (Vec4f, Vec4f, Vec4f) ToAxes()
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

        var x_axis = new Vec4f(1f - (yy + zz), xy + wz, xz - wy, 0f);
        var y_axis = new Vec4f(xy - wz, 1f - (xx + zz), yz + wx, 0f);
        var z_axis = new Vec4f(xz + wy, yz - wx, 1f - (xx + yy), 0f);
        return (x_axis, y_axis, z_axis);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat AxisAngle(Vec3f axis, float angle)
    {
        (float sin, float cos) = (Math.Deg2Rad(angle) * float.CreateSaturating(0.5f)).SinCos();

        var v = axis * sin;
        return new(v.X, v.Y, v.Z, cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat Identity()
    {
        return new(0f, 0f, 0f, 1f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat FromMat4(Mat4f matrix)
    {
        return FromRotationAxes(matrix.Col0.Truncate(), matrix.Col1.Truncate(), matrix.Col2.Truncate());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat FromRotationAxes(Vec3f x, Vec3f y, Vec3f z)
    {
        var (m00, m01, m02) = (x.X, x.Y, x.Z);
        var (m10, m11, m12) = (y.X, y.Y, y.Z);
        var (m20, m21, m22) = (z.X, z.Y, z.Z);

        if (m22 <= 0f)
        {
            var diff10 = m11 - m00;
            var omm22 = 1f - m22;
            if (diff10 <= 0f)
            {
                var fourXsq = omm22 - diff10;
                var inv4x = float.CreateChecked(0.5f) / fourXsq.Sqrt();
                return new(fourXsq * inv4x, (m01 + m10) * inv4x, (m02 + m20) * inv4x, (m12 - m21) * inv4x);
            }
            else
            {
                var fourYsq = omm22 - diff10;
                var inv4y = float.CreateChecked(0.5f) / fourYsq.Sqrt();
                return new((m01 + m10) * inv4y, fourYsq * inv4y, (m12 + m21) * inv4y, (m20 - m02) * inv4y);
            }
        }
        else
        {
            var sum10 = m11 + m00;
            var opp22 = 1f + m22;
            if (sum10 <= 0f)
            {
                var fourXsq = opp22 - sum10;
                var inv4x = float.CreateChecked(0.5f) / fourXsq.Sqrt();
                return new((m02 + m20) * inv4x, (m12 + m21) * inv4x, fourXsq * inv4x, (m01 - m10) * inv4x);
            }
            else
            {
                var fourYsq = opp22 + sum10;
                var inv4y = float.CreateChecked(0.5f) / fourYsq.Sqrt();
                return new((m12 - m21) * inv4y, (m20 - m02) * inv4y, (m01 - m10) * inv4y, fourYsq * inv4y);
            }
        }
    }
}