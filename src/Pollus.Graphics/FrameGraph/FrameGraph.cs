namespace Pollus.Graphics;

using System.Buffers;
using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Graphics.Rendering;

public partial struct FrameGraph<TParam> : IDisposable
{
    public delegate void BuilderDelegate<TData>(ref Builder builder, in TParam param, ref TData data)
        where TData : struct;

    public delegate void ExecuteDelegate<TData>(RenderContext context, in TParam param, in TData data)
        where TData : struct;

    int[]? executionOrder;
    GraphData<PassNode> passNodes;
    GraphData<ResourceNode> resourceNodes;
    FramePassContainer<TParam> passes;
    ResourceContainers resources;

    public ref ResourceContainers Resources => ref Unsafe.AsRef(ref resources);

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
        if (passNodes.Count > 256)
            throw new InvalidOperationException($"Frame graph supports at most 256 passes, got {passNodes.Count}");

        Span<BitSet256> adjacencyMatrix = stackalloc BitSet256[passNodes.Count];
        passNodes.Nodes.Sort();

        foreach (ref var current in passNodes.Nodes)
        {
            ref var edges = ref adjacencyMatrix[current.Index];
            foreach (ref var other in passNodes.Nodes)
            {
                if (current.Index == other.Index) continue;

                if (current.Pass.PassOrder < other.Pass.PassOrder)
                {
                    if (current.Writes.HasAny(other.Reads)
                     || current.Reads.HasAny(other.Writes)
                     || current.Writes.HasAny(other.Writes))
                    {
                        edges.Set(other.Index);
                    }
                }
                else if (current.Pass.PassOrder == other.Pass.PassOrder)
                {
                    if (current.Writes.HasAny(other.Reads))
                    {
                        edges.Set(other.Index);
                    }
                }
            }
        }

        executionOrder = ArrayPool<int>.Shared.Rent(passNodes.Count);
        var executionOrderSpan = executionOrder.AsSpan(0, passNodes.Count);
        for (int i = 0; i < passNodes.Count; i++) executionOrderSpan[i] = passNodes.Nodes[i].Index;

        int orderIndex = 0;
        Span<bool> visited = stackalloc bool[passNodes.Count];
        Span<bool> onStack = stackalloc bool[passNodes.Count];
        foreach (var node in passNodes.Nodes)
        {
            if (!visited[node.Index])
            {
                if (!DFS(node.Index, ref visited, ref onStack, adjacencyMatrix, executionOrderSpan, ref orderIndex))
                {
                    throw new Exception("Cyclic dependency detected");
                }
            }
        }

        executionOrderSpan.Reverse();
        return new FrameGraphRunner<TParam>(ref Unsafe.AsRef(in this), executionOrder.AsSpan(0, passNodes.Count));

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

    public void AddPass<TData, TOrder>(TOrder order, in TParam param, BuilderDelegate<TData> build, ExecuteDelegate<TData> execute)
        where TData : struct
        where TOrder : unmanaged, Enum
    {
        var passHandle = passes.AddPass(new(), order.GetHashCode(), execute);
        var pass = (FramePassContainer<TParam, TData>)passes.GetPass(passHandle);

        ref var passNode = ref passNodes.CreateNode(typeof(TData).Name);
        passNode.SetPass(passHandle);

        var builder = new Builder(ref passNode, ref this);
        build(ref builder, param, ref pass.Get().Data);
    }

    public readonly void ExecutePass(int passIndex, RenderContext renderContext, in TParam param)
    {
        passes.ExecutePass(new(passIndex, 0), renderContext, param);
    }

    public ResourceHandle<TResource> AddResource<TResource>(in TResource resource)
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

    public ResourceHandle<TextureResource> AddTexture(in TextureResource texture)
    {
        return resources.AddTexture(texture);
    }

    public ResourceHandle<BufferResource> AddBuffer(in BufferResource buffer)
    {
        return resources.AddBuffer(buffer);
    }

    public ResourceHandle GetResourceHandle(string name)
    {
        return resources.GetHandle(name);
    }
}