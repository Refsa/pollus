namespace Pollus.Graphics;

using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using Pollus.Collections;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public struct FrameGraph<TExecuteParam> : IDisposable
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
        passNodes.Clear();
        resourceNodes.Clear();
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

                if (current.Reads.HasAny(other.Writes)) edges.Set(other.Index);
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

        return new FrameGraphRunner<TExecuteParam>(this, order.ToArray());

        static bool DFS(int node, ref Span<bool> visited, ref Span<bool> onStack, in Span<BitSet256> adjacencyMatrix, in Span<int> order, ref int orderIndex)
        {
            visited[node] = true;
            onStack[node] = true;

            foreach (var adj in adjacencyMatrix[node])
            {
                if (!visited[adj])
                {
                    if (!DFS(adj, ref visited, ref onStack, adjacencyMatrix, order, ref orderIndex))
                    {
                        return false;
                    }
                }
                else if (onStack[adj])
                {
                    return false;
                }
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

    public ResourceHandle AddResource<TResource>(TResource resource)
    {
        if (resource is TextureDescriptor texture)
        {
            return resources.AddTexture(texture);
        }
        else if (resource is BufferDescriptor buffer)
        {
            return resources.AddBuffer(buffer);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public ResourceHandle GetResourceHandle(string name)
    {
        return resources.GetHandle(name);
    } 

    public ref struct Builder
    {
        FrameGraph<TExecuteParam> frameGraph;
        ref PassNode passNode;

        public Builder(ref PassNode node, FrameGraph<TExecuteParam> frameGraph)
        {
            this.frameGraph = frameGraph;
            passNode = ref node;
        }

        public ResourceHandle Creates<TResource>(TResource resource)
        {
            var handle = frameGraph.AddResource(resource);
            passNode.SetCreate(handle);
            return handle;
        }

        public ResourceHandle Writes(ResourceHandle handle)
        {
            passNode.SetWrite(handle);
            return handle;
        }

        public ResourceHandle Writes(string name)
        {
            var handle = frameGraph.GetResourceHandle(name);
            passNode.SetWrite(handle);
            return handle;
        }

        public ResourceHandle Reads(ResourceHandle handle)
        {
            passNode.SetRead(handle);
            return handle;
        }

        public ResourceHandle Reads(string name)
        {
            var handle = frameGraph.GetResourceHandle(name);
            passNode.SetRead(handle);
            return handle;
        }
    }
}