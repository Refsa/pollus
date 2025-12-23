namespace Pollus.Engine.Serialization;

using Pollus.Utils;
using Pollus.Engine.Assets;
using Pollus.Core.Serialization;
using Pollus.ECS;
using System.Runtime.CompilerServices;

public partial struct SerializeTag : IComponent
{
}

public struct WorldSerializationContext
{
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
    public Handle<T> Deserialize<TReader>(ref TReader reader, in WorldSerializationContext context)
        where TReader : IReader, allows ref struct
    {
        var path = reader.ReadString("$path");
        
        if (string.IsNullOrEmpty(path))
        {
            var asset = reader.Deserialize<T>();
            return context.AssetServer.Assets.Add(asset);
        }

        return context.AssetServer.Load<T>(path);
    }

    public void Serialize<TWriter>(ref TWriter writer, in Handle<T> value, in WorldSerializationContext context)
        where TWriter : IWriter, allows ref struct
    {
        if (value.IsNull()) return;

        var path = context.AssetServer.Assets.GetPath(value);
        if (path.HasValue)
        {
            writer.Write(path.Value.Path, "$path");
        }
        else
        {
            var asset = context.AssetServer.Assets.Get(value);
            if (asset is not null)
            {
                writer.Serialize(asset);
            }
        }
    }
}