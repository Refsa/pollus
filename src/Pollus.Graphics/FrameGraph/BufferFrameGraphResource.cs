namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public struct BufferFrameResource : IFrameGraphResource
{
    public static FrameGraphResourceType Type => FrameGraphResourceType.Buffer;

    public BufferDescriptor Descriptor { get; }
    public string Name { get; }

    public BufferFrameResource(string name, BufferDescriptor descriptor)
    {
        Descriptor = descriptor;
        Name = name;
    }
}
