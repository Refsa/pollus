using System.Buffers;
using Pollus.Collections;

namespace Pollus.Graphics;

public interface INode
{
    int Index { get; }
    string Name { get; }

    void Init(int index, string name);
}

public struct PassNode : INode, IDisposable
{
    BitSet256 reads;
    BitSet256 writes;
    BitSet256 creates;

    public int Index { get; private set; }
    public string Name { get; private set; }
    public FramePassHandle Pass { get; private set; }

    public readonly BitSet256 Reads => reads;
    public readonly BitSet256 Writes => writes;

    public void Dispose()
    {
        
    }

    public void Init(int index, string name)
    {
        Index = index;
        Name = name;
        reads = new();
        writes = new();
        creates = new();
    }

    public void SetPass(in FramePassHandle pass)
    {
        Pass = pass;
    }

    public void SetRead(in ResourceHandle resource)
    {
        reads.Set(resource.Id);
    }

    public void SetWrite(in ResourceHandle resource)
    {
        writes.Set(resource.Id);
    }

    public void SetCreate(in ResourceHandle resource)
    {
        creates.Set(resource.Id);
        SetWrite(resource);
    }
}

public struct ResourceNode : INode
{
    public int Index { get; private set; }
    public string Name { get; private set; }
    public int ProducerIndex { get; private set; }
    public ResourceType Type { get; private set; }
    public ResourceHandle Resource { get; private set; }

    public void Init(int index, string name)
    {
        Index = index;
        Name = name;
    }

    public void SetResource(ResourceType type, ResourceHandle resource)
    {
        Type = type;
        Resource = resource;
    }

    public void SetProducer(int index)
    {
        ProducerIndex = index;
    }
}

public class GraphData<TNode> : IDisposable
    where TNode : struct, INode
{
    TNode[] nodes = ArrayPool<TNode>.Shared.Rent(1);
    int count;

    public Span<TNode> Nodes => nodes.AsSpan(0, count);
    public int Count => count;

    public void Dispose()
    {
        Clear();
        ArrayPool<TNode>.Shared.Return(nodes);
    }

    public void Clear()
    {
        count = 0;
        foreach (var node in nodes)
        {
            (node as IDisposable)?.Dispose();
        }
        Array.Fill(nodes, default);
    }

    public ref TNode CreateNode(string name)
    {
        if (count == nodes.Length)
            Resize();

        ref var node = ref nodes[count++];
        node.Init(count - 1, name);
        return ref node;
    }

    void Resize()
    {
        var newArray = ArrayPool<TNode>.Shared.Rent(nodes.Length * 2);
        nodes.CopyTo(newArray, 0);
        ArrayPool<TNode>.Shared.Return(nodes);
        nodes = newArray;
    }
}