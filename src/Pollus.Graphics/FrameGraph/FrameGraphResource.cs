namespace Pollus.Graphics;

public interface IFrameResource
{
    int Index { get; }
    string Name { get; }
    bool IsTransient { get; }

    public IReadOnlySet<int> ReadPasses { get; }
    public IReadOnlySet<int> WritePasses { get; }

    void AddReadPass(int passIndex);
    void AddWritePass(int passIndex);
}

public abstract class FrameResource : IFrameResource
{
    public enum Type
    {
        Texture,
        Buffer,
    }

    HashSet<int> readPasses = new();
    HashSet<int> writePasses = new();

    public int Index { get; }
    public string Name { get; }
    public Type ResourceType { get; }
    public bool IsTransient { get; }

    public IReadOnlySet<int> ReadPasses => readPasses;
    public IReadOnlySet<int> WritePasses => writePasses;


    public FrameResource(int index, string name, Type type)
    {
        Index = index;
        Name = name;
        ResourceType = type;
    }

    public void AddReadPass(int passIndex)
    {
        readPasses.Add(passIndex);
    }

    public void AddWritePass(int passIndex)
    {
        writePasses.Add(passIndex);
    }
}
