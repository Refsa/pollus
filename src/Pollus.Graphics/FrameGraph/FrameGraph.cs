namespace Pollus.Graphics;

using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Graphics.Rendering;

public partial struct FrameGraph<TExecuteParam> : IDisposable
{
    public delegate void BuilderDelegate<TData>(ref Builder builder, ref TData data);
    public delegate void ExecuteDelegate<TData>(RenderContext context, TExecuteParam renderAssets, TData data);

    GraphData<PassNode> passNodes;
    GraphData<ResourceNode> resourceNodes;

    FramePassContainer<TExecuteParam> passes;

    ResourceContainers resources;

    public FrameGraph()
    {
        // TODO: recycle
        passNodes = new();
        resourceNodes = new();
        passes = new();
        resources = new();
    }

    public void Dispose()
    {
        // TODO: recycle
        passNodes.Dispose();
        resourceNodes.Dispose();
        
        passes.Clear();
        resources.Clear();
    }

    public FrameGraphRunner<TExecuteParam> Compile()
    {
        Span<BitSet256> adjacencyMatrix = stackalloc BitSet256[passNodes.Count];

        foreach (ref var current in passNodes.Nodes)
        {
            ref var edges = ref adjacencyMatrix[current.Index];
            foreach (ref var other in passNodes.Nodes)
            {
                if (current.Index == other.Index) continue;

                if (current.Writes.HasAny(other.Reads)) edges.Set(other.Index);
            }
        }

        Span<int> order = stackalloc int[passNodes.Count];
        int orderIndex = 0;
        Span<bool> visited = stackalloc bool[passNodes.Count];
        Span<bool> onStack = stackalloc bool[passNodes.Count];
        foreach (var node in passNodes.Nodes)
        {
            var adj = adjacencyMatrix[node.Index];
            if (!visited[node.Index])
            {
                if (!DFS(node.Index, ref visited, ref onStack, adjacencyMatrix, order, ref orderIndex))
                {
                    throw new Exception("Cyclic dependency detected");
                }
            }
        }

        order.Reverse();
        return new FrameGraphRunner<TExecuteParam>(this, order.ToArray());

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

    public void AddPass<TData>(string name, BuilderDelegate<TData> build, ExecuteDelegate<TData> execute)
        where TData : struct
    {
        var passHandle = passes.AddPass(new(), execute);
        var pass = (FramePassContainer<TExecuteParam, TData>)passes.GetPass(passHandle);

        ref var passNode = ref passNodes.CreateNode(name);
        passNode.SetPass(passHandle);

        var builder = new Builder(ref passNode, this);
        build(ref builder, ref pass.Get().Data);
    }

    public void ExecutePass(int passIndex, RenderContext renderContext, TExecuteParam param)
    {
        passes.ExecutePass(passIndex, renderContext, param);
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