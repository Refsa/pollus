namespace Pollus.Graphics.Rendering;

public interface IGPUResource<TResource, TDescriptor>
    where TResource : notnull
    where TDescriptor : struct
{
    public TResource? Resource { get; set; }
    public TDescriptor Descriptor { get; set; }
    public int Hash { get; set; }
    public ResourceHandle Handle { get; set; }
}

public struct TextureGPUResource : IGPUResource<GPUTexture, TextureDescriptor>
{
    public int Hash { get; set; }
    public ResourceHandle Handle { get; set; }

    public GPUTexture? Resource { get; set; }
    public GPUTextureView TextureView { get; set; }
    public TextureDescriptor Descriptor { get; set; }

    public TextureGPUResource(GPUTexture? texture, GPUTextureView textureView, TextureDescriptor descriptor)
    {
        Resource = texture;
        TextureView = textureView;
        Descriptor = descriptor;
        Hash = descriptor.GetHashCode();
    }
}

public struct BufferGPUResource : IGPUResource<GPUBuffer, BufferDescriptor>
{
    public int Hash { get; set; }
    public ResourceHandle Handle { get; set; }
    public GPUBuffer? Resource { get; set; }
    public BufferDescriptor Descriptor { get; set; }

    public BufferGPUResource(GPUBuffer resource, BufferDescriptor descriptor)
    {
        Resource = resource;
        Descriptor = descriptor;
        Hash = descriptor.GetHashCode();
    }
}

public record struct ResourceMeta
{
    public required ResourceHandle Handle { get; init; }
    public required string Label { get; init; }
    public required int Index { get; init; }
    public required int Hash { get; init; }
}

public class RenderResourceCache
{
    List<TextureGPUResource> textures = [];
    List<BufferGPUResource> buffers = [];

    Dictionary<ResourceHandle, ResourceMeta> lookup = [];

    public bool Has(ResourceHandle handle) => lookup.ContainsKey(handle);

    public ResourceHandle SetTexture(ResourceHandle handle, TextureGPUResource resource)
    {
        if (lookup.TryGetValue(handle, out var meta))
        {
            textures[meta.Index] = resource;
        }
        else
        {
            textures.Add(resource);
            resource.Handle = handle;

            lookup[handle] = new()
            {
                Label = resource.Descriptor.Label,
                Handle = handle,
                Index = textures.Count - 1,
                Hash = resource.Hash
            };
        }
        return handle;
    }

    public ResourceHandle SetBuffer(ResourceHandle handle, BufferGPUResource resource)
    {
        if (lookup.TryGetValue(handle, out var meta))
        {
            buffers[meta.Index] = resource;
        }
        else
        {
            buffers.Add(resource);
            resource.Handle = handle;

            lookup[handle] = new()
            {
                Label = resource.Descriptor.Label,
                Handle = handle,
                Index = buffers.Count - 1,
                Hash = resource.Hash
            };
        }
        return handle;
    }

    public TextureGPUResource GetTexture(ResourceHandle handle)
    {
        if (!lookup.TryGetValue(handle, out var meta)) throw new Exception("Resource not found");
        if (meta.Handle.Type != ResourceType.Texture) throw new Exception("Resource type mismatch");
        return textures[meta.Index];
    }

    public BufferGPUResource GetBuffer(ResourceHandle handle)
    {
        if (!lookup.TryGetValue(handle, out var meta)) throw new Exception("Resource not found");
        if (meta.Handle.Type != ResourceType.Buffer) throw new Exception("Resource type mismatch");
        return buffers[meta.Index];
    }

    public TextureGPUResource RemoveTexture(ResourceHandle resourceHandle)
    {
        if (!lookup.TryGetValue(resourceHandle, out var meta)) throw new Exception("Resource not found");
        if (meta.Handle.Type != ResourceType.Texture) throw new Exception("Resource type mismatch");

        var index = meta.Index;
        var resource = textures[index];
        textures.RemoveAt(index);
        lookup.Remove(resourceHandle);

        for (int i = index; i < textures.Count; i++)
        {
            var current = lookup[textures[i].Handle];
            lookup[textures[i].Handle] = current with
            {
                Index = i
            };
        }

        return resource;
    }

    public BufferGPUResource RemoveBuffer(ResourceHandle resourceHandle)
    {
        if (!lookup.TryGetValue(resourceHandle, out var meta)) throw new Exception("Resource not found");
        if (meta.Handle.Type != ResourceType.Buffer) throw new Exception("Resource type mismatch");

        var index = meta.Index;
        var resource = buffers[index];
        buffers.RemoveAt(index);
        lookup.Remove(resourceHandle);

        for (int i = index; i < buffers.Count; i++)
        {
            var current = lookup[buffers[i].Handle];
            lookup[buffers[i].Handle] = current with
            {
                Index = i
            };
        }

        return resource;
    }
}
