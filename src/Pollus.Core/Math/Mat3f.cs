namespace Pollus.Mathematics;

using System.Diagnostics;
using Pollus.Graphics;

[ShaderType]
[DebuggerDisplay("Mat3f: {Col0}, {Col1}, {Col2}")]
public partial record struct Mat3f
{
    public Vec3f Col0;
    public Vec3f Col1;
    public Vec3f Col2;

    public Mat3f(Vec3f col0, Vec3f col1, Vec3f col2)
    {
        Col0 = col0;
        Col1 = col1;
        Col2 = col2;
    }

    public Mat3f(float m00, float m01, float m02,
                 float m10, float m11, float m12,
                 float m20, float m21, float m22)
    {
        Col0 = new(m00, m01, m02);
        Col1 = new(m10, m11, m12);
        Col2 = new(m20, m21, m22);
    }

    public void WithTranslation(in Vec2f translation)
    {
        Col2.X = translation.X;
        Col2.Y = translation.Y;
    }

    public void WithScale(in Vec2f scale)
    {
        Col0.X = scale.X;
        Col1.Y = scale.Y;
    }

    public void WithRotation(float angle)
    {
        var c = MathF.Cos(angle);
        var s = MathF.Sin(angle);
        Col0.X = c;
        Col0.Y = -s;
        Col1.X = s;
        Col1.Y = c;
    }

    public void Translate(in Vec2f translation)
    {
        Col2.X += translation.X;
        Col2.Y += translation.Y;
    }

    public void Scale(in Vec2f scale)
    {
        Col0.X *= scale.X;
        Col1.Y *= scale.Y;
    }

    public void Rotate(float angle)
    {
        var c = MathF.Cos(angle);
        var s = MathF.Sin(angle);
        var m00 = Col0.X * c - Col0.Y * s;
        var m01 = Col0.X * s + Col0.Y * c;
        var m10 = Col1.X * c - Col1.Y * s;
        var m11 = Col1.X * s + Col1.Y * c;
        Col0.X = m00;
        Col0.Y = m01;
        Col1.X = m10;
        Col1.Y = m11;
    }

    public static Mat3f operator *(in Mat3f left, in Mat3f right)
    {
        return new(
            left.Col0 * right.Col0.X + left.Col1 * right.Col0.Y + left.Col2 * right.Col0.Z,
            left.Col0 * right.Col1.X + left.Col1 * right.Col1.Y + left.Col2 * right.Col1.Z,
            left.Col0 * right.Col2.X + left.Col1 * right.Col2.Y + left.Col2 * right.Col2.Z
        );
    }

    public static Mat3f operator *(in Mat3f left, in Vec3f right)
    {
        return new(
            left.Col0 * right.X,
            left.Col1 * right.Y,
            left.Col2 * right.Z
        );
    }

    public static Mat3f operator *(in Vec3f left, in Mat3f right)
    {
        return new(
            left.X * right.Col0,
            left.Y * right.Col1,
            left.Z * right.Col2
        );
    }

    public static Mat3f operator *(in Mat3f left, float right)
    {
        return new(
            left.Col0 * right,
            left.Col1 * right,
            left.Col2 * right
        );
    }

    public static Mat3f operator *(float left, in Mat3f right)
    {
        return new(
            right.Col0 * left,
            right.Col1 * left,
            right.Col2 * left
        );
    }

    public static Mat3f operator +(in Mat3f left, in Mat3f right)
    {
        return new(
            left.Col0 + right.Col0,
            left.Col1 + right.Col1,
            left.Col2 + right.Col2
        );
    }

    public static Mat3f operator -(in Mat3f left, in Mat3f right)
    {
        return new(
            left.Col0 - right.Col0,
            left.Col1 - right.Col1,
            left.Col2 - right.Col2
        );
    }

    public static Mat3f Identity()
    {
        return new(
            1f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f
        );
    }

    public static Mat3f FromTRS(Vec2f translation, float rotation, Vec2f scale)
    {
        var cos = MathF.Cos(rotation);
        var sin = MathF.Sin(rotation);
        return new(
            scale.X * cos, -scale.Y * sin, 0f,
            scale.X * sin, scale.Y * cos, 0f,
            translation.X, translation.Y, 1f
        );
    }

    public static Mat3f FromScale(Vec2f scale)
    {
        return new(
            scale.X, 0f, 0f,
            0f, scale.Y, 0f,
            0f, 0f, 1f
        );
    }

    public static Mat3f FromRotation(float radians)
    {
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);
        return new(
            cos, -sin, 0f,
            sin, cos, 0f,
            0f, 0f, 1f
        );
    }

    public static Mat3f FromTranslation(Vec2f translation)
    {
        return new(
            1f, 0f, 0f,
            0f, 1f, 0f,
            translation.X, translation.Y, 1f
        );
    }

    public Mat3f Transpose()
    {
        return new(
            new Vec3f(Col0.X, Col1.X, Col2.X),
            new Vec3f(Col0.Y, Col1.Y, Col2.Y),
            new Vec3f(Col0.Z, Col1.Z, Col2.Z)
        );
    }
}