namespace Pollus.Engine.Serialization;

using Core.Assets;
using Pollus.Core.Serialization;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Utils;

public partial struct SerializeTag : IComponent
{
}

public struct WorldSerializationContext()
{
    public required AssetServer AssetServer { get; set; }
    public HashSet<Handle> Dependencies { get; set; } = [];
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
    where T : IAsset
{
    public Handle<T> Deserialize<TReader>(ref TReader reader, in WorldSerializationContext context)
        where TReader : IReader, allows ref struct
    {
        var path = reader.ReadString("$path");

        Handle handle;
        if (string.IsNullOrEmpty(path))
        {
            var asset = reader.Deserialize<T>();
            handle = context.AssetServer.AddAsync(asset);
        }
        else
        {
            handle = context.AssetServer.LoadAsync<T>(path);
        }

        context.Dependencies.Add(handle);
        return handle;
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
            var asset = context.AssetServer.Assets.GetAsset(value);
            if (asset is not null)
            {
                writer.Serialize(asset);
            }
        }
    }
}