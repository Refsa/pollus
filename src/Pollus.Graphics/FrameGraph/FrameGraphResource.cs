namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public enum ResourceType
{
    Unknown = 0,
    Texture,
    Buffer,
}

public readonly record struct ResourceHandle(int Id, int Index);
public readonly record struct ResourceHandle<TResource>(int Id, int Index)
{
    public static implicit operator ResourceHandle(ResourceHandle<TResource> handle) => new(handle.Id, handle.Index);
    public static implicit operator ResourceHandle<TResource>(ResourceHandle handle) => new(handle.Id, handle.Index);
}

public class ResourceContainer<TResource>
    where TResource : struct
{
    TResource[] resources = new TResource[1];
    int count;

    public ResourceHandle<TResource> Add(int id, TResource resource)
    {
        if (count == resources.Length) Resize();
        resources[count++] = resource;
        return new ResourceHandle(id, count - 1);
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
    ResourceContainer<TextureDescriptor> textures;
    ResourceContainer<BufferDescriptor> buffers;
    Dictionary<string, ResourceHandle> resourceMap;

    public ResourceContainers()
    {
        textures = new();
        buffers = new();
        resourceMap = new();
    }

    public void Clear()
    {
        count = 0;
        textures.Clear();
        buffers.Clear();
    }

    public ResourceHandle GetHandle(string label)
    {
        return resourceMap[label];
    }

    public ResourceHandle<TextureDescriptor> AddTexture(TextureDescriptor texture)
    {
        var handle = textures.Add(count++, texture);
        resourceMap.Add(texture.Label, handle);
        return handle;
    }

    public ResourceHandle<BufferDescriptor> AddBuffer(BufferDescriptor buffer)
    {
        var handle = buffers.Add(count++, buffer);
        resourceMap.Add(buffer.Label, handle);
        return handle;
    }

    public ref TextureDescriptor GetTexture(ResourceHandle<TextureDescriptor> handle)
    {
        return ref textures.Get(handle);
    }

    public ref BufferDescriptor GetBuffer(ResourceHandle<BufferDescriptor> handle)
    {
        return ref buffers.Get(handle);
    }
}