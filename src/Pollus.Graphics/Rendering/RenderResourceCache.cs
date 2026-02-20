namespace Pollus.Graphics.Rendering;

using Pollus.Collections;

public interface IGPUResource<TResource, TDescriptor>
    where TResource : notnull
    where TDescriptor : struct
{
    public TResource? Resource { get; set; }
    public TDescriptor Descriptor { get; set; }
    public int Hash { get; set; }
    public ResourceHandle Handle { get; set; }
}

public struct TextureGPUResource : IGPUResource<GPUTexture, TextureDescriptor>, IDisposable
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

    public void Dispose()
    {
        if (Resource is null) return;
        Resource.Dispose();
        TextureView.Dispose();
    }
}

public struct BufferGPUResource : IGPUResource<GPUBuffer, BufferDescriptor>, IDisposable
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

    public void Dispose()
    {
        Resource?.Dispose();
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
    ArrayList<TextureGPUResource> textures = [];
    ArrayList<BufferGPUResource> buffers = [];

    Dictionary<ResourceHandle, ResourceMeta> lookup = [];

    public bool Has(ResourceHandle handle) => lookup.ContainsKey(handle);

    public void Cleanup()
    {
        foreach (scoped ref var texture in textures.AsSpan())
        {
            texture.Dispose();
        }
        foreach (scoped ref var buffer in buffers.AsSpan())
        {
            buffer.Dispose();
        }
        textures.Clear();
        buffers.Clear();
        lookup.Clear();
    }

    public void ReleaseTexture(ResourceHandle handle)
    {
        if (!lookup.TryGetValue(handle, out var meta)) throw new Exception("Resource not found");

        textures[meta.Index].Dispose();
        RemoveTexture(handle);
    }

    public void ReleaseBuffer(ResourceHandle handle)
    {
        if (!lookup.TryGetValue(handle, out var meta)) throw new Exception("Resource not found");

        buffers[meta.Index].Dispose();
        RemoveBuffer(handle);
    }

    public ResourceHandle SetTexture(ResourceHandle handle, TextureGPUResource resource)
    {
        if (lookup.TryGetValue(handle, out var meta))
        {
            if (textures[meta.Index].Handle.Type != ResourceType.Unknown)
            {
                textures[meta.Index].Dispose();
            }
            textures[meta.Index] = resource;
        }
        else
        {
            resource.Handle = handle;
            textures.Add(resource);

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
            if (buffers[meta.Index].Handle.Type != ResourceType.Unknown)
            {
                buffers[meta.Index].Dispose();
            }
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

    public ref TextureGPUResource GetTexture(ResourceHandle handle)
    {
        if (!lookup.TryGetValue(handle, out var meta)) throw new Exception("Resource not found");
        if (meta.Handle.Type != ResourceType.Texture) throw new Exception("Resource type mismatch");
        return ref textures.Get(meta.Index);
    }

    public ref BufferGPUResource GetBuffer(ResourceHandle handle)
    {
        if (!lookup.TryGetValue(handle, out var meta)) throw new Exception("Resource not found");
        if (meta.Handle.Type != ResourceType.Buffer) throw new Exception("Resource type mismatch");
        return ref buffers.Get(meta.Index);
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
