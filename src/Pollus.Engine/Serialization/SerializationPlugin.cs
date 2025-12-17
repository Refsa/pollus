using System.Runtime.CompilerServices;

namespace Pollus.Engine.Serialization;

using Pollus.Utils;
using Pollus.Engine.Assets;
using Pollus.Core.Serialization;
using Pollus.ECS;

public partial struct SerializeTag : IComponent
{
}

public struct WorldSerializationContext
{
    public Resources Resources { get; set; }
    public AssetServer AssetServer { get; set; }
}

[SystemSet]
public partial class SerializationPlugin<TSerialization> : IPlugin
    where TSerialization : ISerialization, new()
{
    public void Apply(World world)
    {
        world.Resources.Add<ISerialization>(new TSerialization());
    }
}

public class HandleSerializer<T> : IBlittableSerializer<Handle<T>, WorldSerializationContext>
    where T : notnull
{
    public Handle<T> Deserialize<TReader>(ref TReader reader, in WorldSerializationContext context) where TReader : IReader
    {
        var path = reader.ReadString();
        return context.AssetServer.Load<T>(path);
    }

    public void Serialize<TWriter>(ref TWriter writer, ref Handle<T> value, in WorldSerializationContext context) where TWriter : IWriter
    {
    }
}

public class HandleSerializer : IBlittableSerializer<Handle, WorldSerializationContext>
{
    public Handle Deserialize<TReader>(ref TReader reader, in WorldSerializationContext context) where TReader : IReader
    {
        var type = reader.ReadString();
        var path = reader.ReadString();
        var c = new Handle();
        return c;
    }

    public void Serialize<TWriter>(ref TWriter writer, ref Handle value, in WorldSerializationContext context) where TWriter : IWriter
    {
    }
}