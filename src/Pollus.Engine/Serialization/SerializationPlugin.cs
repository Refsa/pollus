namespace Pollus.Engine.Serialization;

using Pollus.Core.Serialization;
using Pollus.ECS;

public struct SerializeTag : IComponent
{
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
