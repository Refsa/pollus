namespace Pollus.Mathematics;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Pollus.Core.Serialization;
using Pollus.Utils;

[Reflect, Serialize]
[DebuggerDisplay("Quat: {EulerAngleX}, {EulerAngleY}, {EulerAngleZ}")]
public record struct Quat
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public float EulerAngleX => Math.Radians(X).Degrees() * 2f;
    public float EulerAngleY => Math.Radians(Y).Degrees() * 2f;
    public float EulerAngleZ => Math.Radians(Z).Degrees() * 2f;

    public Quat(float x, float y, float z, float w)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.W = w;
    }

    public Quat(Vec3f v, float w) : this(v.X, v.Y, v.Z, w)
    {
    }

    public Quat(Vec4f f) : this(f.X, f.Y, f.Z, f.W)
    {
    }

    public static Quat operator *(Quat q1, Quat q2) => new(
        q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y,
        q1.W * q2.Y + q1.Y * q2.W + q1.Z * q2.X - q1.X * q2.Z,
        q1.W * q2.Z + q1.Z * q2.W + q1.X * q2.Y - q1.Y * q2.X,
        q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z
    );

    public static Vec3f operator *(Quat q, Vec3f v) => new(
        v.X * q.W + v.Y * q.Z - v.Z * q.Y + q.X * v.X + q.Y * v.Z - q.Z * v.Y,
        -v.X * q.Z + v.Y * q.W + v.Z * q.X + q.X * v.X - q.Y * v.Z + q.Z * v.Y,
        v.X * q.Y - v.Y * q.X + v.Z * q.W + q.X * v.X + q.Y * v.Y - q.Z * v.X
    );

    public float LengthSquared()
    {
        return X * X + Y * Y + Z * Z + W * W;
    }

    public bool IsNormalized()
    {
        return Math.Abs(LengthSquared() - 1f) < 0.0001f;
    }

    public Quat Normalized()
    {
        var length = LengthSquared();
        if (length == 0f) return this;
        var invLength = 1f / length;
        return new Quat(X * invLength, Y * invLength, Z * invLength, W * invLength);
    }

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
    public static Quat AxisAngle(Vec3f axis, float radians)
    {
        var halfRads = radians * 0.5f;
        var sin = Math.Sin(halfRads);
        return new(
            axis * sin,
            Math.Cos(halfRads)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quat FromEuler(Vec3f eulerAngles)
    {
        var (c1, s1) = Math.Radians(eulerAngles.X * 0.5f).SinCos();
        var (c2, s2) = Math.Radians(eulerAngles.Y * 0.5f).SinCos();
        var (c3, s3) = Math.Radians(eulerAngles.Z * 0.5f).SinCos();

        var c1c2 = c1 * c2;
        var s1s2 = s1 * s2;

        return new(
            c1c2 * s3 + s1s2 * c3,
            s1 * c2 * c3 + c1 * s2 * s3,
            c1 * s2 * c3 - s1 * c2 * s3,
            c1c2 * c3 - s1s2 * s3
        );
    }

    public static Vec3f ToEuler(Quat q)
    {
        return new(
            q.EulerAngleX,
            q.EulerAngleY,
            q.EulerAngleZ
        );
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