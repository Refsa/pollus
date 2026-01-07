namespace Pollus.Utils;

using Core.Serialization;
using Pollus.Graphics;
using Pollus.Mathematics;

[ShaderType, Reflect]
public partial struct Color
{
    public static readonly Color TRANSPARENT = new(0f, 0f, 0f, 0f);
    public static readonly Color BLACK = new(0f, 0f, 0f, 1f);
    public static readonly Color WHITE = new(1f, 1f, 1f, 1f);
    public static readonly Color AMARANTH = new(229f / 255f, 43f / 255f, 80f / 255f);
    public static readonly Color AMBER = new(255f / 255f, 191f / 255f, 0);
    public static readonly Color AMETHYST = new(153f / 255f, 102f / 255f, 204f / 255f);
    public static readonly Color APRICOT = new(251f / 255f, 206f / 255f, 177f / 255f);
    public static readonly Color AQUAMARINE = new(127f / 255f, 255f / 255f, 212f / 255f);
    public static readonly Color AZURE = new(0, 127f / 255f, 255f / 255f);
    public static readonly Color BABY_BLUE = new(137f / 255f, 207f / 255f, 240f / 255f);
    public static readonly Color BEIGE = new(245f / 255f, 245f / 255f, 220f / 255f);
    public static readonly Color BRICK_RED = new(203f / 255f, 65f / 255f, 84f / 255f);
    public static readonly Color BLUE = new(0, 0, 255f / 255f);
    public static readonly Color BLUE_GREEN = new(0, 149f / 255f, 182f / 255f);
    public static readonly Color BLUE_VIOLET = new(138f / 255f, 43f / 255f, 226f / 255f);
    public static readonly Color BLUSH = new(222f / 255f, 93f / 255f, 131f / 255f);
    public static readonly Color BRONZE = new(205f / 255f, 127f / 255f, 50f / 255f);
    public static readonly Color BROWN = new(150f / 255f, 75f / 255f, 0);
    public static readonly Color BURGUNDY = new(128f / 255f, 0, 32f / 255f);
    public static readonly Color BYZANTIUM = new(112f / 255f, 41f / 255f, 99f / 255f);
    public static readonly Color CARMINE = new(150f / 255f, 0, 24f / 255f);
    public static readonly Color CERISE = new(222f / 255f, 49f / 255f, 99f / 255f);
    public static readonly Color CERULEAN = new(0, 123f / 255f, 167f / 255f);
    public static readonly Color CHAMPAGNE = new(247f / 255f, 231f / 255f, 206f / 255f);
    public static readonly Color CHARTREUSE_GREEN = new(127f / 255f, 255f / 255f, 0);
    public static readonly Color CHOCOLATE = new(123f / 255f, 63f / 255f, 0);
    public static readonly Color COBALT_BLUE = new(0, 71f / 255f, 171f / 255f);
    public static readonly Color COFFEE = new(111f / 255f, 78f / 255f, 55f / 255f);
    public static readonly Color COPPER = new(184f / 255f, 115f / 255f, 51f / 255f);
    public static readonly Color CORAL = new(255f / 255f, 127f / 255f, 80f / 255f);
    public static readonly Color CRIMSON = new(220f / 255f, 20f / 255f, 60f / 255f);
    public static readonly Color CYAN = new(0, 255f / 255f, 255f / 255f);
    public static readonly Color DESERT_SAND = new(237f / 255f, 201f / 255f, 175f / 255f);
    public static readonly Color ELECTRIC_BLUE = new(125f / 255f, 249f / 255f, 255f / 255f);
    public static readonly Color EMERALD = new(80f / 255f, 200f / 255f, 120f / 255f);
    public static readonly Color ERIN = new(0, 255f / 255f, 63f / 255f);
    public static readonly Color GOLD = new(255f / 255f, 215f / 255f, 0);
    public static readonly Color GRAY = new(128f / 255f, 128f / 255f, 128f / 255f);
    public static readonly Color GREEN = new(0, 128f / 255f, 0);
    public static readonly Color HARLEQUIN = new(63f / 255f, 255f / 255f, 0);
    public static readonly Color INDIGO = new(75f / 255f, 0, 130f / 255f);
    public static readonly Color IVORY = new(255f / 255f, 255f / 255f, 240f / 255f);
    public static readonly Color JADE = new(0, 168f / 255f, 107f / 255f);
    public static readonly Color JUNGLE_GREEN = new(41f / 255f, 171f / 255f, 135f / 255f);
    public static readonly Color LAVENDER = new(181f / 255f, 126f / 255f, 220f / 255f);
    public static readonly Color LEMON = new(255f / 255f, 247f / 255f, 0);
    public static readonly Color LILAC = new(200f / 255f, 162f / 255f, 200f / 255f);
    public static readonly Color LIME = new(191f / 255f, 255f / 255f, 0);
    public static readonly Color MAGENTA = new(255f / 255f, 0, 255f / 255f);
    public static readonly Color MAGENTA_ROSE = new(255f / 255f, 0, 175f / 255f);
    public static readonly Color MAROON = new(128f / 255f, 0, 0);
    public static readonly Color MAUVE = new(224f / 255f, 176f / 255f, 255f / 255f);
    public static readonly Color NAVY_BLUE = new(0, 0, 128f / 255f);
    public static readonly Color OCHRE = new(204f / 255f, 119f / 255f, 34f / 255f);
    public static readonly Color OLIVE = new(128f / 255f, 128f / 255f, 0);
    public static readonly Color ORANGE = new(255f / 255f, 102f / 255f, 0);
    public static readonly Color ORANGE_RED = new(255f / 255f, 69f / 255f, 0);
    public static readonly Color ORCHID = new(218f / 255f, 112f / 255f, 214f / 255f);
    public static readonly Color PEACH = new(255f / 255f, 229f / 255f, 180f / 255f);
    public static readonly Color PEAR = new(209f / 255f, 226f / 255f, 49f / 255f);
    public static readonly Color PERIWINKLE = new(204f / 255f, 204f / 255f, 255f / 255f);
    public static readonly Color PERSIAN_BLUE = new(28f / 255f, 57f / 255f, 187f / 255f);
    public static readonly Color PINK = new(253f / 255f, 108f / 255f, 158f / 255f);
    public static readonly Color PLUM = new(142f / 255f, 69f / 255f, 133f / 255f);
    public static readonly Color PRUSSIAN_BLUE = new(0, 49f / 255f, 83f / 255f);
    public static readonly Color PUCE = new(204f / 255f, 136f / 255f, 153f / 255f);
    public static readonly Color PURPLE = new(128f / 255f, 0, 128f / 255f);
    public static readonly Color RASPBERRY = new(227f / 255f, 11f / 255f, 92f / 255f);
    public static readonly Color RED = new(255f / 255f, 0, 0);
    public static readonly Color RED_VIOLET = new(199f / 255f, 21f / 255f, 133f / 255f);
    public static readonly Color ROSE = new(255f / 255f, 0, 127f / 255f);
    public static readonly Color RUBY = new(224f / 255f, 17f / 255f, 95f / 255f);
    public static readonly Color SALMON = new(250f / 255f, 128f / 255f, 114f / 255f);
    public static readonly Color SANGRIA = new(146f / 255f, 0, 10f / 255f);
    public static readonly Color SAPPHIRE = new(15f / 255f, 82f / 255f, 186f / 255f);
    public static readonly Color SCARLET = new(255f / 255f, 36f / 255f, 0);
    public static readonly Color SILVER = new(192f / 255f, 192f / 255f, 192f / 255f);
    public static readonly Color SLATE_GRAY = new(112f / 255f, 128f / 255f, 144f / 255f);
    public static readonly Color SPRING_BUD = new(167f / 255f, 252f / 255f, 0);
    public static readonly Color SPRING_GREEN = new(0, 255f / 255f, 127f / 255f);
    public static readonly Color TAN = new(210f / 255f, 180f / 255f, 140f / 255f);
    public static readonly Color TAUPE = new(72f / 255f, 60f / 255f, 50f / 255f);
    public static readonly Color TEAL = new(0, 128f / 255f, 128f / 255f);
    public static readonly Color TURQUOISE = new(64f / 255f, 224f / 255f, 208f / 255f);
    public static readonly Color ULTRAMARINE = new(63f / 255f, 0, 255f / 255f);
    public static readonly Color VIOLET = new(127f / 255f, 0, 255f / 255f);
    public static readonly Color VIRIDIAN = new(64f / 255f, 130f / 255f, 109f / 255f);
    public static readonly Color YELLOW = new(255f / 255f, 255f / 255f, 0);

    Vec4f color;

    public float R
    {
        get => color.X;
        set => color.X = value;
    }

    public float G
    {
        get => color.Y;
        set => color.Y = value;
    }

    public float B
    {
        get => color.Z;
        set => color.Z = value;
    }

    public float A
    {
        get => color.W;
        set => color.W = value;
    }

    public Color(Hex hex) : this(hex, hex.A)
    {
    }

    public Color(Hex hex, float alpha)
    {
        this.color = new Vec4f(hex.R, hex.G, hex.B, alpha);
    }

    public Color(RGB rgb) : this(rgb.R, rgb.G, rgb.B, 1f)
    {
    }

    public Color(RGB rgb, float a) : this(rgb.R, rgb.G, rgb.B, a)
    {
    }

    public Color(float r, float g, float b) : this(r, g, b, 1f)
    {
    }

    public Color(float r, float g, float b, float a)
    {
        this.color = new Vec4f(r, g, b, a);
    }

    public Color(HSV hsv) : this(hsv, 1f)
    {
    }

    public Color(HSV hsv, float a)
    {
        var rgb = (RGB)hsv;
        this.color = new Vec4f(rgb.R, rgb.G, rgb.B, a);
    }

    public static implicit operator Color(Vec4<float> vec4)
    {
        return new Color(vec4.X, vec4.Y, vec4.Z, vec4.W);
    }

    public static implicit operator Color(System.Numerics.Vector4 vec4)
    {
        return new()
        {
            color = new Vec4f(vec4.X, vec4.Y, vec4.Z, vec4.W)
        };
    }

    public static implicit operator Vec4<float>(Color col)
    {
        return new Vec4<float>(col.R, col.G, col.B, col.A);
    }

    public static implicit operator Vec4<double>(Color col)
    {
        return new Vec4<double>(col.R, col.G, col.B, col.A);
    }

    public static implicit operator Vec4f(in Color col)
    {
        return new Vec4f(col.R, col.G, col.B, col.A);
    }

    public static implicit operator System.Numerics.Vector4(Color col)
    {
        return new System.Numerics.Vector4(col.R, col.G, col.B, col.A);
    }

    public Color Darken(float by)
    {
        return (Vec4<float>)this * (1f - by);
    }

    public Color Lighten(float by)
    {
        return (Vec4<float>)this * (1f + by);
    }

    public readonly Color WithAlpha(float a)
    {
        var copy = this;
        copy.A = a;
        return copy;
    }

    public Color HueShift(float p)
    {
        var hsv = (HSV)new RGB(R, G, B);
        hsv.H += p;
        if (hsv.H < 0f) hsv.H += 1f;
        else if (hsv.H > 1f) hsv.H -= 1f;
        return new(hsv, A);
    }

    public override string ToString()
    {
        return $"Color {{ {R}, {G}, {B}, {A} }}";
    }

    public struct Hex
    {
        public static readonly Hex Black = new(0x00000000);
        public static readonly Hex White = new(0xFFFFFF00);

        public uint Value { get; set; }

        public float R => (float)((Value & 0xFF000000) >> 24) / 0xFF;
        public float G => (float)((Value & 0x00FF0000) >> 16) / 0xFF;
        public float B => (float)((Value & 0x0000FF00) >> 8) / 0xFF;
        public float A => (float)(Value & 0x000000FF) / 0xFF;

        /// <summary>
        /// 0xRRGGBB00 or 0xRRGGBBAA
        /// </summary>
        public Hex(uint value)
        {
            Value = value;
        }

        public static implicit operator RGB(Hex hex)
        {
            return new RGB(hex.R, hex.G, hex.B);
        }

        public static implicit operator Color(Hex hex)
        {
            return new Color(hex);
        }

        public static implicit operator Vec4<float>(Hex hex)
        {
            return new Vec4<float>(hex.R, hex.G, hex.B, hex.A);
        }

        public static implicit operator Vec4<double>(Hex hex)
        {
            return new Vec4<double>(hex.R, hex.G, hex.B, hex.A);
        }
    }

    public struct HSV
    {
        public float H { get; set; }
        public float S { get; set; }
        public float V { get; set; }

        public HSV(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }

        public static implicit operator Color(HSV hsv)
        {
            return new Color(hsv);
        }

        public static implicit operator RGB(HSV hsv)
        {
            if (hsv.S == 0f) return new RGB(hsv.V, hsv.V, hsv.V);
            var i = (int)(hsv.H * 6f);
            var f = (hsv.H * 6f) - i;
            var p = hsv.V * (1f - hsv.S);
            var q = hsv.V * (1f - hsv.S * f);
            var t = hsv.V * (1f - hsv.S * (1f - f));
            i = i % 6;

            return i switch
            {
                0 => new RGB(hsv.V, t, p),
                1 => new RGB(q, hsv.V, p),
                2 => new RGB(p, hsv.V, t),
                3 => new RGB(p, q, hsv.V),
                4 => new RGB(t, p, hsv.V),
                5 => new RGB(hsv.V, p, q),
                _ => throw new Exception(),
            };
        }
    }

    public struct RGB
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        public RGB(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static implicit operator Color(RGB rgb)
        {
            return new Color(rgb.R, rgb.G, rgb.B);
        }

        public static implicit operator HSV(RGB rgb)
        {
            var max = float.Max(rgb.R, float.Max(rgb.G, rgb.B));
            var min = float.Min(rgb.R, float.Min(rgb.G, rgb.B));
            var delta = max - min;

            var v = max;
            if (min.Approximately(max, float.Epsilon)) return new HSV(0f, 0f, v);

            var s = delta / max;
            var rc = (max - rgb.R) / delta;
            var gc = (max - rgb.G) / delta;
            var bc = (max - rgb.B) / delta;

            float h;
            if (rgb.R.Approximately(max, float.Epsilon))
            {
                h = 0f + bc - gc;
            }
            else if (rgb.G.Approximately(max, float.Epsilon))
            {
                h = 2f + rc - bc;
            }
            else
            {
                h = 4f + gc - rc;
            }

            h = (h / 6f) % 1f;
            return new HSV(h, s, v);
        }
    }
}

public struct Color8
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public static implicit operator Color8(Color color)
    {
        return new Color8
        {
            R = (byte)(color.R * 255),
            G = (byte)(color.G * 255),
            B = (byte)(color.B * 255),
            A = (byte)(color.A * 255)
        };
    }
}

public class ColorSerializer : IBlittableSerializer<Color, DefaultSerializationContext>
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void ModuleInitializer()
    {
        BlittableSerializerLookup<DefaultSerializationContext>.RegisterSerializer(new ColorSerializer());
    }

    public Color Deserialize<TReader>(ref TReader reader, in DefaultSerializationContext context) where TReader : IReader, allows ref struct
    {
        return new(
            reader.Read<float>("R"),
            reader.Read<float>("G"),
            reader.Read<float>("B"),
            reader.Read<float>("A")
        );
    }

    public void Serialize<TWriter>(ref TWriter writer, in Color value, in DefaultSerializationContext context) where TWriter : IWriter, allows ref struct
    {
        writer.Write(value.R, "R");
        writer.Write(value.G, "G");
        writer.Write(value.B, "B");
        writer.Write(value.A, "A");
    }
}