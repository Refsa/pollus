namespace Pollus.Engine.Mathematics;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

[StructLayout(LayoutKind.Explicit)]
public struct Float3
{
    [FieldOffset(0)]
    internal Vector128<float> inner;

    public float X
    {
        get => inner.GetElement(0);
        set => inner = inner.WithElement(0, value);
    }

    public float Y
    {
        get => inner.GetElement(1);
        set => inner = inner.WithElement(1, value);
    }

    public float Z
    {
        get => inner.GetElement(2);
        set => inner = inner.WithElement(2, value);
    }

    public Float3(float x, float y, float z)
    {
        inner = Vector128.Create(x, y, z, 0f);
    }

    public Float3(Vector128<float> inner)
    {
        this.inner = inner;
    }

    public static Float3 operator +(in Float3 a, in Float3 b)
    {
        return a.Add(b);
    }
}

public static class Float3Ops
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Float3 Add(this in Float3 a, in Float3 b)
    {
        return new Float3(Sse.Add(a.inner, b.inner));
    }
}