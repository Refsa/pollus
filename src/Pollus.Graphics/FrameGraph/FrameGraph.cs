namespace Pollus.Graphics;

using System.Buffers;
using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Graphics.Rendering;

public partial struct FrameGraph<TParam> : IDisposable
{
    public delegate void BuilderDelegate<TData>(ref Builder builder, TParam param, ref TData data);
    public delegate void ExecuteDelegate<TData>(RenderContext context, TParam param, TData data);

    int[]? executionOrder;
    GraphData<PassNode> passNodes;
    GraphData<ResourceNode> resourceNodes;
    FramePassContainer<TParam> passes;
    ResourceContainers resources;

    public ResourceContainers Resources => resources;

    public FrameGraph()
    {
        passNodes = new();
        resourceNodes = new();
        passes = new();
        resources = new();
    }

    public void Dispose()
    {
        passNodes.Dispose();
        resourceNodes.Dispose();
        resources.Dispose();
        passes.Dispose();

        if (executionOrder != null)
        {
            ArrayPool<int>.Shared.Return(executionOrder);
            executionOrder = null;
        }
    }

    public FrameGraphRunner<TParam> Compile()
    {
        Span<BitSet256> adjacencyMatrix = stackalloc BitSet256[passNodes.Count];
        passNodes.Nodes.Sort(static (a, b) => b.Pass.PassOrder.CompareTo(a.Pass.PassOrder));

        foreach (ref var current in passNodes.Nodes)
        {
            ref var edges = ref adjacencyMatrix[current.Index];
            foreach (ref var other in passNodes.Nodes)
            {
                if (current.Index == other.Index) continue;

                if (current.Writes.HasAny(other.Reads)) edges.Set(other.Index);
            }
        }

        executionOrder = ArrayPool<int>.Shared.Rent(passNodes.Count);
        var executionOrderSpan = executionOrder.AsSpan(0, passNodes.Count);
        for (int i = 0; i < passNodes.Count; i++) executionOrderSpan[i] = passNodes.Nodes[i].Index;
        passNodes.Nodes.Sort(executionOrderSpan, static (a, b) => b.Pass.PassOrder.CompareTo(a.Pass.PassOrder));

        int orderIndex = 0;
        Span<bool> visited = stackalloc bool[passNodes.Count];
        Span<bool> onStack = stackalloc bool[passNodes.Count];
        foreach (var node in passNodes.Nodes)
        {
            var adj = adjacencyMatrix[node.Index];
            if (!visited[node.Index])
            {
                if (!DFS(node.Index, ref visited, ref onStack, adjacencyMatrix, executionOrderSpan, ref orderIndex))
                {
                    throw new Exception("Cyclic dependency detected");
                }
            }
        }

        executionOrderSpan.Reverse();
        return new FrameGraphRunner<TParam>(this, executionOrderSpan);

        static bool DFS(int node, ref Span<bool> visited, ref Span<bool> onStack, in Span<BitSet256> adjacencyMatrix, in Span<int> order, ref int orderIndex)
        {
            visited[node] = true;
            onStack[node] = true;

            foreach (var adj in adjacencyMatrix[node])
            {
                if (visited[adj] && onStack[adj]) return false;
                if (visited[adj]) continue;

                var ok = DFS(adj, ref visited, ref onStack, adjacencyMatrix, order, ref orderIndex);
                if (!ok) return false;
            }

            onStack[node] = false;
            order[orderIndex++] = node;
            return true;
        }
    }

    public void AddPass<TData, TOrder>(TOrder order, TParam param, BuilderDelegate<TData> build, ExecuteDelegate<TData> execute)
        where TData : struct
        where TOrder : struct, Enum, IConvertible
    {
        var passHandle = passes.AddPass(new(), order.ToInt32(null), execute);
        var pass = (FramePassContainer<TParam, TData>)passes.GetPass(passHandle);

        ref var passNode = ref passNodes.CreateNode(typeof(TData).Name);
        passNode.SetPass(passHandle);

        var builder = new Builder(ref passNode, ref this);
        build(ref builder, param, ref pass.Get().Data);
    }

    public void ExecutePass(int passIndex, RenderContext renderContext, TParam param)
    {
        passes.ExecutePass(new(passIndex, 0), renderContext, param);
    }

    public ResourceHandle<TResource> AddResource<TResource>(TResource resource)
        where TResource : struct, IFrameGraphResource
    {
        if (resource is TextureResource texture)
        {
            var handle = resources.AddTexture(texture);
            return Unsafe.As<ResourceHandle<TextureResource>, ResourceHandle<TResource>>(ref handle);
        }
        else if (resource is BufferResource buffer)
        {
            var handle = resources.AddBuffer(buffer);
            return Unsafe.As<ResourceHandle<BufferResource>, ResourceHandle<TResource>>(ref handle);
        }
        throw new Exception("Unknown resource type");
    }

    public ResourceHandle<TextureResource> AddTexture(TextureResource texture)
    {
        return resources.AddTexture(texture);
    }

    public ResourceHandle<BufferResource> AddBuffer(BufferResource buffer)
    {
        return resources.AddBuffer(buffer);
    }

    public ResourceHandle GetResourceHandle(string name)
    {
        return resources.GetHandle(name);
    }
}