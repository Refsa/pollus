namespace Pollus.Graphics;

using System.Buffers;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

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

    ResourceHandle Handle { get; set; }
    string Label { get; }
    int Hash { get; }
}

public struct TextureResource : IFrameGraphResource
{
    public static ResourceType Type => ResourceType.Texture;

    public string Label { get; }
    public ResourceHandle Handle { get; set; }
    public TextureDescriptor Descriptor { get; }
    public int Hash { get; }

    public TextureResource(string label, TextureDescriptor descriptor)
    {
        Label = label;
        Descriptor = descriptor;
        Hash = HashCode.Combine(
            descriptor.Dimension, descriptor.Format,
            descriptor.Size, descriptor.MipLevelCount,
            descriptor.SampleCount, descriptor.Usage,
            descriptor.ViewFormats
        );
    }

    public static implicit operator TextureResource(TextureDescriptor descriptor) => new(descriptor.Label, descriptor);
}

public struct BufferResource : IFrameGraphResource
{
    public static ResourceType Type => ResourceType.Buffer;

    public string Label { get; }
    public ResourceHandle Handle { get; set; }
    public BufferDescriptor Descriptor { get; }
    public int Hash { get; }

    public BufferResource(string label, BufferDescriptor descriptor)
    {
        Label = label;
        Descriptor = descriptor;
        Hash = HashCode.Combine(descriptor.Size, descriptor.Usage);
    }

    public static implicit operator BufferResource(BufferDescriptor descriptor) => new(descriptor.Label, descriptor);
}

public struct ResourceContainer<TResource> : IDisposable
    where TResource : struct, IFrameGraphResource
{
    TResource[] resources = ArrayPool<TResource>.Shared.Rent(1);
    int count;

    public ReadOnlySpan<TResource> Resources => resources.AsSpan(0, count);

    public ResourceContainer() { }

    public void Dispose()
    {
        Array.Fill(resources, default, 0, count);
        ArrayPool<TResource>.Shared.Return(resources);
    }

    public ResourceHandle<TResource> Add(int id, TResource resource)
    {
        if (count == resources.Length) Resize();
        resource.Handle = new ResourceHandle<TResource>(id, count);
        resources[count++] = resource;
        return resource.Handle;
    }

    public ref TResource Get(ResourceHandle<TResource> handle)
    {
        return ref resources[handle.Index];
    }

    void Resize()
    {
        var newArray = ArrayPool<TResource>.Shared.Rent(resources.Length * 2);
        resources.CopyTo(newArray, 0);
        ArrayPool<TResource>.Shared.Return(resources);
        resources = newArray;
    }
}

public struct ResourceContainers : IDisposable
{
    int count;
    ResourceContainer<TextureResource> textures;
    ResourceContainer<BufferResource> buffers;
    Dictionary<string, ResourceHandle> resourceByName;

    public IReadOnlyDictionary<string, ResourceHandle> ResourceByName => resourceByName;
    public ResourceContainer<TextureResource> Textures => textures;
    public ResourceContainer<BufferResource> Buffers => buffers;

    public ResourceContainers()
    {
        textures = new();
        buffers = new();
        resourceByName = Pool<Dictionary<string, ResourceHandle>>.Shared.Rent();
    }

    public void Dispose()
    {
        count = 0;
        textures.Dispose();
        buffers.Dispose();
        resourceByName.Clear();
        Pool<Dictionary<string, ResourceHandle>>.Shared.Return(resourceByName);
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