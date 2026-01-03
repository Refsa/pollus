namespace Pollus.Graphics;

using System.Runtime.InteropServices;
using Pollus.Collections;

[StructLayout(LayoutKind.Sequential)]
public struct DrawNode : IComparable<DrawNode>
{
    public ulong SortKey;
    public int RendererID;
    public int BatchID;
    public int InstanceIndex;

    public int CompareTo(DrawNode other)
    {
        return SortKey.CompareTo(other.SortKey);
    }
}

public class DrawQueue
{
    ArrayList<DrawNode> nodes;

    public int Count => nodes.Count;
    public Span<DrawNode> Nodes => nodes.AsSpan();

    public DrawQueue(int initialCapacity = 1024)
    {
        nodes = new ArrayList<DrawNode>(initialCapacity);
    }

    public void Add(ulong sortKey, int rendererID, int batchID, int instanceIndex)
    {
        nodes.Add(new DrawNode
        {
            SortKey = sortKey,
            RendererID = rendererID,
            BatchID = batchID,
            InstanceIndex = instanceIndex,
        });
    }

    public void Sort()
    {
        nodes.AsSpan().Sort();
    }

    public void Clear()
    {
        nodes.Clear();
    }
}
