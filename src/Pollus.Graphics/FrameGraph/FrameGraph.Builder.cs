namespace Pollus.Graphics;

public partial struct FrameGraph<TExecuteParam>
{
    public ref struct Builder
    {
        ref FrameGraph<TExecuteParam> frameGraph;
        ref PassNode passNode;

        public Builder(ref PassNode node, ref FrameGraph<TExecuteParam> frameGraph)
        {
            this.frameGraph = ref frameGraph;
            passNode = ref node;
        }

        public ResourceHandle<TResource> Creates<TResource>(TResource resource)
            where TResource : struct, IFrameGraphResource
        {
            var handle = frameGraph.AddResource(resource);
            passNode.SetCreate(handle);
            return handle;
        }

        public ResourceHandle<TResource> Writes<TResource>(in ResourceHandle<TResource> handle)
            where TResource : struct, IFrameGraphResource
        {
            passNode.SetWrite(handle);
            return handle;
        }

        public ResourceHandle<TResource> Writes<TResource>(string name)
            where TResource : struct, IFrameGraphResource
        {
            var handle = frameGraph.GetResourceHandle(name);
            passNode.SetWrite(handle);
            return handle;
        }

        public ResourceHandle<TResource> Reads<TResource>(in ResourceHandle<TResource> handle)
            where TResource : struct, IFrameGraphResource
        {
            passNode.SetRead(handle);
            return handle;
        }

        public ResourceHandle<TResource> Reads<TResource>(string name)
            where TResource : struct, IFrameGraphResource
        {
            var handle = frameGraph.GetResourceHandle(name);
            passNode.SetRead(handle);
            return handle;
        }
    }
}