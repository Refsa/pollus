namespace Pollus.Mathematics;

using Pollus.Core.Serialization;
using System.Runtime.CompilerServices;

public sealed class RectIntSerializer : IBlittableSerializer<Rect<int>, DefaultSerializationContext>
{
    [ModuleInitializer]
    public static void Register()
    {
        BlittableSerializerLookup<DefaultSerializationContext>.RegisterSerializer(new Vec2Serializer<int>());
        BlittableSerializerLookup<DefaultSerializationContext>.RegisterSerializer(new RectIntSerializer());
    }

    public Rect<int> Deserialize<TReader>(ref TReader reader, in DefaultSerializationContext context) where TReader : IReader, allows ref struct
    {
        var min = reader.Read<Vec2<int>>("Min");
        var max = reader.Read<Vec2<int>>("Max");
        return Rect<int>.FromMinMax(min, max);
    }

    public void Serialize<TWriter>(ref TWriter writer, in Rect<int> value, in DefaultSerializationContext context) where TWriter : IWriter, allows ref struct
    {
        writer.Write(value.Min, "Min");
        writer.Write(value.Max, "Max");
    }
}
