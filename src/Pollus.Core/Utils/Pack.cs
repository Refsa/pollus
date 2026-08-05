namespace Pollus.Utils;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics;

public static class PackUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackUnorm8x4(in Color color)
    {
        var r = (uint)(color.R * 255f + 0.5f);
        var g = (uint)(color.G * 255f + 0.5f);
        var b = (uint)(color.B * 255f + 0.5f);
        var a = (uint)(color.A * 255f + 0.5f);

        return r | (g << 8) | (b << 16) | (a << 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color UnpackUnorm8x4(uint value)
    {
        const float scale = 1f / 255f;

        return new Color(
            (value & 0xff) * scale,
            ((value >> 8) & 0xff) * scale,
            ((value >> 16) & 0xff) * scale,
            ((value >> 24) & 0xff) * scale
        );
    }
}

[StructLayout(LayoutKind.Sequential), ShaderType]
public partial struct PackedU16x4
{
    public ushort X;
    public ushort Y;
    public ushort Z;
    public ushort W;

    public PackedU16x4(float x, float y, float z, float w)
    {
        X = ToUInt16(x);
        Y = ToUInt16(y);
        Z = ToUInt16(z);
        W = ToUInt16(w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ToUInt16(float value)
    {
        return (ushort)value;
    }
}