namespace Pollus.Graphics.Rendering;

using Pollus.Debugging;

public class RenderResourceCache
{
    Dictionary<ResourceHandle, GPUTextureView> textureViews = new();
    Dictionary<ResourceHandle, GPUBuffer> bufferViews = new();

    public void AddTextureView(ResourceHandle handle, GPUTextureView view)
    {
        textureViews[handle] = view;
    }

    public void AddBufferView(ResourceHandle handle, GPUBuffer buffer)
    {
        bufferViews[handle] = buffer;
    }

    public GPUTextureView GetTextureView(ResourceHandle handle)
    {
        Guard.IsTrue(textureViews.TryGetValue(handle, out var view), "TextureView not found");
        return view;
    }

    public GPUBuffer GetBufferView(ResourceHandle handle)
    {
        Guard.IsTrue(bufferViews.TryGetValue(handle, out var buffer), "BufferView not found");
        return buffer!;
    }

    public GPUTextureView RemoveTextureView(ResourceHandle handle)
    {
        Guard.IsTrue(textureViews.TryGetValue(handle, out var view), "TextureView not found");
        textureViews.Remove(handle);
        return view;
    }

    public GPUBuffer RemoveBufferView(ResourceHandle handle)
    {
        Guard.IsTrue(bufferViews.TryGetValue(handle, out var buffer), "BufferView not found");
        bufferViews.Remove(handle);
        return buffer!;
    }
}
