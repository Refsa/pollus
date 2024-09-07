namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public struct TextureFrameResource : IFrameGraphResource
{
    public static FrameGraphResourceType Type => FrameGraphResourceType.Texture;

    public TextureDescriptor Descriptor { get; }
    public string Name { get; }

    public TextureFrameResource(string name, TextureDescriptor descriptor)
    {
        Descriptor = descriptor;
        Name = name;
    }
}