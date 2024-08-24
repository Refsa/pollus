namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public class BufferFrameGraphResource : FrameResource
{
    public struct Descriptor
    {
        public string Label;
        public BufferUsage Usage;
        public ulong Size;
        public bool MappedAtCreation;
    }

    Descriptor descriptor;

    public Descriptor BufferDescriptor => descriptor;

    public BufferFrameGraphResource(int index, string name) : base(index, name, Type.Buffer)
    {
    }

    public void SetDescriptor(Descriptor descriptor)
    {
        this.descriptor = descriptor;
    }
}
