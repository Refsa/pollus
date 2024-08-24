namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public class TextureFrameGraphResource : FrameResource
{
    public struct Descriptor
    {
        public string Label;

        public TextureUsage Usage;
        public TextureDimension Dimension;
        public Extent3D Size;
        public TextureFormat Format;

        public uint MipLevelCount;
        public uint SampleCount;
        public TextureFormat[] ViewFormats;
    }

    Descriptor textureDescriptor;

    public Descriptor TextureDescriptor => textureDescriptor;

    public TextureFrameGraphResource(int index, string name) : base(index, name, Type.Texture)
    {
    }

    public void SetDescriptor(Descriptor descriptor)
    {
        this.textureDescriptor = descriptor;
    }
}