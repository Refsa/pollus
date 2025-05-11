namespace Pollus.Engine.Serialization;

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Pollus.Core.Serialization;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Utils;

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

public interface ISerialization
{
    struct WriterWrapper : IDisposable
    {
        ISerialization serialization;
        public IWriter Writer { get; }

        public WriterWrapper(ISerialization serialization, IWriter writer)
        {
            this.serialization = serialization;
            Writer = writer;
        }

        public void Dispose()
        {
            serialization.Return(Writer);
        }
    }

    struct ReaderWrapper : IDisposable
    {
        ISerialization serialization;
        public IReader Reader { get; }

        public ReaderWrapper(ISerialization serialization, IReader reader)
        {
            this.serialization = serialization;
            Reader = reader;
        }

        public void Dispose()
        {
            Reader.Init(null);
            serialization.Return(Reader);
        }
    }

    WriterWrapper Writer { get; }
    ReaderWrapper Reader { get; }

    void Return(IWriter writer);
    void Return(IReader reader);
}
