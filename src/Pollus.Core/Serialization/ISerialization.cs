namespace Pollus.Core.Serialization;

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