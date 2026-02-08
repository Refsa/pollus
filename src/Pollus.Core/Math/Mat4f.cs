namespace Pollus.Mathematics;

using Pollus.Core.Serialization;
using Pollus.Utils;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Pollus.Debugging;
using Pollus.Graphics;

[ShaderType, Reflect, Serialize]
[DebuggerDisplay("Mat4f: {Col0}, {Col1}, {Col2}, {Col3}")]
public partial record struct Mat4f
{
    static readonly Mat4f identity = new(
        1f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f,
        0f, 0f, 1f, 0f,
        0f, 0f, 0f, 1f
    );

    public Vec4f Col0;
    public Vec4f Col1;
    public Vec4f Col2;
    public Vec4f Col3;

    public Vec3f Left => new(Col0.X, Col0.Y, Col0.Z);
    public Vec3f Right => new(Col0.X, Col0.Y, Col0.Z);
    public Vec3f Up => new(Col1.X, Col1.Y, Col1.Z);
    public Vec3f Down => new(Col1.X, Col1.Y, Col1.Z);
    public Vec3f Forward => new(Col2.X, Col2.Y, Col2.Z);
    public Vec3f Back => new(Col2.X, Col2.Y, Col2.Z);

    public Vec3f Translation => new(Col3.X, Col3.Y, Col3.Z);
    public Quat Rotation => Quat.FromMat4(this);
    public Vec3f Scale => new(Col0.Length(), Col1.Length(), Col2.Length());


    public Mat4f(Vec4f row0, Vec4f row1, Vec4f row2, Vec4f row3)
    {
        Col0 = row0;
        Col1 = row1;
        Col2 = row2;
        Col3 = row3;
    }

    public Mat4f(float m00, float m01, float m02, float m03,
        float m10, float m11, float m12, float m13,
        float m20, float m21, float m22, float m23,
        float m30, float m31, float m32, float m33)
    {
        Col0 = new(m00, m01, m02, m03);
        Col1 = new(m10, m11, m12, m13);
        Col2 = new(m20, m21, m22, m23);
        Col3 = new(m30, m31, m32, m33);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f operator *(in Mat4f left, in Mat4f right)
    {
        if (!Sse.IsSupported)
        {
            return new(
                left.Col0 * right.Col0.X + left.Col1 * right.Col0.Y + left.Col2 * right.Col0.Z + left.Col3 * right.Col0.W,
                left.Col0 * right.Col1.X + left.Col1 * right.Col1.Y + left.Col2 * right.Col1.Z + left.Col3 * right.Col1.W,
                left.Col0 * right.Col2.X + left.Col1 * right.Col2.Y + left.Col2 * right.Col2.Z + left.Col3 * right.Col2.W,
                left.Col0 * right.Col3.X + left.Col1 * right.Col3.Y + left.Col2 * right.Col3.Z + left.Col3 * right.Col3.W
            );
        }

        var leftCol0 = Unsafe.As<Vec4f, Vector128<float>>(ref Unsafe.AsRef(in left.Col0));
        var leftCol1 = Unsafe.As<Vec4f, Vector128<float>>(ref Unsafe.AsRef(in left.Col1));
        var leftCol2 = Unsafe.As<Vec4f, Vector128<float>>(ref Unsafe.AsRef(in left.Col2));
        var leftCol3 = Unsafe.As<Vec4f, Vector128<float>>(ref Unsafe.AsRef(in left.Col3));

        var col0 = Sse.Add(
            Sse.Add(
                Sse.Multiply(leftCol0, Vector128.Create(right.Col0.X)),
                Sse.Multiply(leftCol1, Vector128.Create(right.Col0.Y))
            ),
            Sse.Add(
                Sse.Multiply(leftCol2, Vector128.Create(right.Col0.Z)),
                Sse.Multiply(leftCol3, Vector128.Create(right.Col0.W))
            )
        );

        var col1 = Sse.Add(
            Sse.Add(
                Sse.Multiply(leftCol0, Vector128.Create(right.Col1.X)),
                Sse.Multiply(leftCol1, Vector128.Create(right.Col1.Y))
            ),
            Sse.Add(
                Sse.Multiply(leftCol2, Vector128.Create(right.Col1.Z)),
                Sse.Multiply(leftCol3, Vector128.Create(right.Col1.W))
            )
        );

        var col2 = Sse.Add(
            Sse.Add(
                Sse.Multiply(leftCol0, Vector128.Create(right.Col2.X)),
                Sse.Multiply(leftCol1, Vector128.Create(right.Col2.Y))
            ),
            Sse.Add(
                Sse.Multiply(leftCol2, Vector128.Create(right.Col2.Z)),
                Sse.Multiply(leftCol3, Vector128.Create(right.Col2.W))
            )
        );

        var col3 = Sse.Add(
            Sse.Add(
                Sse.Multiply(leftCol0, Vector128.Create(right.Col3.X)),
                Sse.Multiply(leftCol1, Vector128.Create(right.Col3.Y))
            ),
            Sse.Add(
                Sse.Multiply(leftCol2, Vector128.Create(right.Col3.Z)),
                Sse.Multiply(leftCol3, Vector128.Create(right.Col3.W))
            )
        );

        return new(Unsafe.As<Vector128<float>, Vec4f>(ref col0),
            Unsafe.As<Vector128<float>, Vec4f>(ref col1),
            Unsafe.As<Vector128<float>, Vec4f>(ref col2),
            Unsafe.As<Vector128<float>, Vec4f>(ref col3));
    }

    public static Vec4f operator *(in Mat4f left, in Vec4f right)
    {
        return new(
            left.Col0.X * right.X + left.Col1.X * right.Y + left.Col2.X * right.Z + left.Col3.X * right.W,
            left.Col0.Y * right.X + left.Col1.Y * right.Y + left.Col2.Y * right.Z + left.Col3.Y * right.W,
            left.Col0.Z * right.X + left.Col1.Z * right.Y + left.Col2.Z * right.Z + left.Col3.Z * right.W,
            left.Col0.W * right.X + left.Col1.W * right.Y + left.Col2.W * right.Z + left.Col3.W * right.W
        );
    }

    public static Mat4f operator *(in Mat4f left, float right)
    {
        return new(
            left.Col0 * right,
            left.Col1 * right,
            left.Col2 * right,
            left.Col3 * right
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f Identity() => identity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f OrthographicRightHanded(float left, float right, float top, float bottom, float near, float far)
    {
        var rcpWidth = 1f / (right - left);
        var rcpHeight = 1f / (top - bottom);
        var rcpDepth = 1f / (near - far);
        return new(
            2f * rcpWidth, 0f, 0f, 0f,
            0f, 2f * rcpHeight, 0f, 0f,
            0f, 0f, rcpDepth, 0f,
            (left + right) * -rcpWidth, (top + bottom) * -rcpHeight, near * rcpDepth, 1f
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f FromTRS(Vec3f translation, Quat rotation, Vec3f scale)
    {
        Guard.IsTrue(rotation.IsNormalized(), "Rotation must be normalized.");

        var (xAxis, yAxis, zAxis) = rotation.ToAxes();
        return new(
            xAxis * scale.X,
            yAxis * scale.Y,
            zAxis * scale.Z,
            new Vec4f(translation, 1f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f FromTRS(Vec2f translation, float rotation, Vec2f scale)
    {
        var (sin, cos) = Math.SinCos(rotation);
        return new(
            new Vec4f(cos * scale.X, sin * scale.X, 0f, 0f),
            new Vec4f(-sin * scale.Y, cos * scale.Y, 0f, 0f),
            new Vec4f(0f, 0f, 1f, 0f),
            new Vec4f(translation.X, translation.Y, 0f, 1f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f FromTRS(Vec3f translation, float rotation, Vec2f scale)
    {
        var (sin, cos) = Math.SinCos(rotation);
        return new(
            new Vec4f(cos * scale.X, sin * scale.X, 0f, 0f),
            new Vec4f(-sin * scale.Y, cos * scale.Y, 0f, 0f),
            new Vec4f(0f, 0f, 1f, 0f),
            new Vec4f(translation.X, translation.Y, translation.Z, 1f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f FromTRS_Row(Vec3f translation, Quat rotation, Vec3f scale)
    {
        Guard.IsTrue(rotation.IsNormalized(), "Rotation must be normalized.");

        var (xAxis, yAxis, zAxis) = rotation.ToAxes();
        return new(
            new Vec4f(xAxis.X * scale.X, yAxis.X * scale.Y, zAxis.X * scale.Z, translation.X),
            new Vec4f(xAxis.Y * scale.X, yAxis.Y * scale.Y, zAxis.Y * scale.Z, translation.Y),
            new Vec4f(xAxis.Z * scale.X, yAxis.Z * scale.Y, zAxis.Z * scale.Z, translation.Z),
            new Vec4f(0f, 0f, 0f, 1f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f FromTRS_Row(Vec2f translation, float rotation, Vec2f scale)
    {
        var (sin, cos) = Math.SinCos(rotation);
        return new(
            new Vec4f(cos * scale.X, sin * scale.X, 0f, translation.X),
            new Vec4f(-sin * scale.Y, cos * scale.Y, 0f, translation.Y),
            new Vec4f(0f, 0f, 1f, 0f),
            new Vec4f(0f, 0f, 0f, 1f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f FromTRS_Row(Vec3f translation, float rotation, Vec2f scale)
    {
        var (sin, cos) = Math.SinCos(rotation);
        return new(
            new Vec4f(cos * scale.X, sin * scale.X, 0f, translation.X),
            new Vec4f(-sin * scale.Y, cos * scale.Y, 0f, translation.Y),
            new Vec4f(0f, 0f, 1f, translation.Z),
            new Vec4f(0f, 0f, 0f, 1f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f FromTR(Vec3f translation, Quat rotation)
    {
        var (xAxis, yAxis, zAxis) = rotation.ToAxes();
        return new(
            xAxis,
            yAxis,
            zAxis,
            new Vec4f(translation, 1f)
        );
    }

    public static Mat4f FromScale(Vec3f scale)
    {
        return new(
            new Vec4f(scale.X, 0f, 0f, 0f),
            new Vec4f(0f, scale.Y, 0f, 0f),
            new Vec4f(0f, 0f, scale.Z, 0f),
            Vec4f.UnitW
        );
    }

    public static Mat4f FromTranslation(Vec3f translation)
    {
        return new(
            Vec4f.UnitX,
            Vec4f.UnitY,
            Vec4f.UnitZ,
            new Vec4f(translation, 1f)
        );
    }

    public static Mat4f FromRotation(Quat quat)
    {
        var (xAxis, yAxis, zAxis) = quat.ToAxes();
        return new(
            xAxis, yAxis, zAxis, Vec4f.UnitW
        );
    }

    public static Mat4f FromRotationX(float radians)
    {
        var (sin, cos) = Math.SinCos(radians);
        return new(
            1f, 0f, 0f, 0f,
            0f, cos, -sin, 0f,
            0f, sin, cos, 0f,
            0f, 0f, 0f, 1f
        );
    }

    public static Mat4f FromRotationY(float radians)
    {
        var (sin, cos) = Math.SinCos(radians);
        return new(
            cos, 0f, sin, 0f,
            0f, 1f, 0f, 0f,
            -sin, 0f, cos, 0f,
            0f, 0f, 0f, 1f
        );
    }

    public static Mat4f FromRotationZ(float radians)
    {
        var (sin, cos) = Math.SinCos(radians);
        return new(
            cos, -sin, 0f, 0f,
            sin, cos, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
        );
    }

    public void Translate(Vec3f translation)
    {
        Col3 += new Vec4f(translation, 0f);
    }

    public void SetTranslation(Vec3f translation)
    {
        Col3 = new Vec4f(translation, Col3.W);
    }

    public Mat4f Translated(Vec3f translation)
    {
        var copy = this;
        copy.Translate(translation);
        return copy;
    }

    public Vec3f GetTranslation()
    {
        return new Vec3f(Col3.X, Col3.Y, Col3.Z);
    }

    public Quat GetRotation()
    {
        return Quat.FromMat4(this);
    }

    public Vec3f GetScale()
    {
        return new Vec3f(Col0.Length(), Col1.Length(), Col2.Length());
    }

    public void SetScale(Vec3f scale)
    {
        Col0 = new Vec4f(scale.X, 0f, 0f, 0f);
        Col1 = new Vec4f(0f, scale.Y, 0f, 0f);
        Col2 = new Vec4f(0f, 0f, scale.Z, 0f);
    }

    public float GetRotationX()
    {
        return Math.Atan2(Col2.Y, Col2.Z);
    }

    public float GetRotationY()
    {
        return Math.Atan2(-Col2.X, Math.Sqrt(Col2.Y * Col2.Y + Col2.Z * Col2.Z));
    }

    public float GetRotationZ()
    {
        return Math.Atan2(Col1.X, Col0.X);
    }

    public readonly Mat4f Transpose()
    {
        return new(
            new Vec4f(Col0.X, Col1.X, Col2.X, Col3.X),
            new Vec4f(Col0.Y, Col1.Y, Col2.Y, Col3.Y),
            new Vec4f(Col0.Z, Col1.Z, Col2.Z, Col3.Z),
            new Vec4f(Col0.W, Col1.W, Col2.W, Col3.W)
        );
    }

    public Mat4f Inverse()
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

        var fac0 = new Vec4f(coef00, coef00, coef02, coef03);
        var fac1 = new Vec4f(coef04, coef04, coef06, coef07);
        var fac2 = new Vec4f(coef08, coef08, coef10, coef11);
        var fac3 = new Vec4f(coef12, coef12, coef14, coef15);
        var fac4 = new Vec4f(coef16, coef16, coef18, coef19);
        var fac5 = new Vec4f(coef20, coef20, coef22, coef23);

        var vec0 = new Vec4f(m10, m00, m00, m00);
        var vec1 = new Vec4f(m11, m01, m01, m01);
        var vec2 = new Vec4f(m12, m02, m02, m02);
        var vec3f = new Vec4f(m13, m03, m03, m03);

        var inv0 = vec1 * fac0 - vec2 * fac1 + vec3f * fac2;
        var inv1 = vec0 * fac0 - vec2 * fac3 + vec3f * fac4;
        var inv2 = vec0 * fac1 - vec1 * fac3 + vec3f * fac5;
        var inv3 = vec0 * fac2 - vec1 * fac4 + vec2 * fac5;

        var sign_a = new Vec4f(1f, -1f, 1f, -1f);
        var sign_b = new Vec4f(-1f, 1f, -1f, 1f);

        var inverse = new Mat4f(
            inv0 * sign_a,
            inv1 * sign_b,
            inv2 * sign_a,
            inv3 * sign_b
        );

        var col0 = new Vec4f(
            inverse.Col0.X,
            inverse.Col1.X,
            inverse.Col2.X,
            inverse.Col3.X
        );

        var dot0 = Col0 * col0;
        var dot1 = dot0.X + dot0.Y + dot0.Z + dot0.W;

        var rcp_det = dot1.Rcp();
        return inverse * rcp_det;
    }
}