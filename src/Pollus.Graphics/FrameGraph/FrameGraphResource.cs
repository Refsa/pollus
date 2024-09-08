namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public enum ResourceType
{
    Unknown = 0,
    Texture,
    Buffer,
}

public readonly record struct ResourceHandle(int Id, int Index, ResourceType Type);
public readonly record struct ResourceHandle<TResource>(int Id, int Index)
    where TResource : struct, IFrameGraphResource
{
    public static implicit operator ResourceHandle(ResourceHandle<TResource> handle) => new(handle.Id, handle.Index, TResource.Type);
    public static implicit operator ResourceHandle<TResource>(ResourceHandle handle) => new(handle.Id, handle.Index);
}

public interface IFrameGraphResource
{
    static abstract ResourceType Type { get; }
    string Label { get; }
}

public struct TextureResource : IFrameGraphResource
{
    public static ResourceType Type => ResourceType.Texture;

    public string Label { get; }
    public TextureDescriptor Descriptor { get; }

    public TextureResource(string label, TextureDescriptor descriptor)
    {
        Label = label;
        Descriptor = descriptor;
    }

    public static implicit operator TextureResource(TextureDescriptor descriptor) => new(descriptor.Label, descriptor);
}

public struct BufferResource : IFrameGraphResource
{
    public static ResourceType Type => ResourceType.Buffer;

    public string Label { get; }
    public BufferDescriptor Descriptor { get; }

    public BufferResource(string label, BufferDescriptor descriptor)
    {
        Label = label;
        Descriptor = descriptor;
    }

    public static implicit operator BufferResource(BufferDescriptor descriptor) => new(descriptor.Label, descriptor);
}

public class ResourceContainer<TResource>
    where TResource : struct, IFrameGraphResource
{
    TResource[] resources = new TResource[1];
    int count;

    public ResourceHandle<TResource> Add(int id, TResource resource)
    {
        if (count == resources.Length) Resize();
        resources[count++] = resource;
        return new ResourceHandle<TResource>(id, count - 1);
    }

    public ref TResource Get(ResourceHandle<TResource> handle)
    {
        return ref resources[handle.Index];
    }

    public void Clear()
    {
        count = 0;
        Array.Fill(resources, default);
    }

    void Resize()
    {
        Array.Resize(ref resources, resources.Length * 2);
    }
}

public class ResourceContainers
{
    int count;
    ResourceContainer<TextureResource> textures;
    ResourceContainer<BufferResource> buffers;
    Dictionary<string, ResourceHandle> resourceByName;

    public IReadOnlyDictionary<string, ResourceHandle> ResourceByName => resourceByName;

    public ResourceContainers()
    {
        textures = new();
        buffers = new();
        resourceByName = new();
    }

    public void Clear()
    {
        count = 0;
        textures.Clear();
        buffers.Clear();
        resourceByName.Clear();
    }

    public ResourceHandle GetHandle(string label)
    {
        return resourceByName[label];
    }

    public ResourceHandle<TextureResource> AddTexture(TextureResource texture)
    {
        var handle = textures.Add(count++, texture);
        resourceByName.Add(texture.Label, handle);
        return handle;
    }

    public ResourceHandle<BufferResource> AddBuffer(BufferResource buffer)
    {
        var handle = buffers.Add(count++, buffer);
        resourceByName.Add(buffer.Label, handle);
        return handle;
    }

    public ref TextureResource GetTexture(ResourceHandle<TextureResource> handle)
    {
        return ref textures.Get(handle);
    }

    public ref BufferResource GetBuffer(ResourceHandle<BufferResource> handle)
    {
        return ref buffers.Get(handle);
    }

    public IFrameGraphResource Get(ResourceHandle handle)
    {
        return handle.Type switch
        {
            ResourceType.Texture => textures.Get(new(handle.Id, handle.Index)),
            ResourceType.Buffer => buffers.Get(new(handle.Id, handle.Index)),
            _ => throw new NotImplementedException(),
        };
    }
}