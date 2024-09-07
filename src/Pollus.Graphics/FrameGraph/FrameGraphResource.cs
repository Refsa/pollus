namespace Pollus.Graphics;

public enum FrameGraphResourceType
{
    Texture = 1,
    Buffer,
}

public interface IFrameGraphResource
{
    public static abstract FrameGraphResourceType Type { get; }
    public string Name { get; }
}

public readonly record struct ResourceHandle<TResource>(int Id, int Hash) where TResource : notnull, IFrameGraphResource;
public readonly record struct FrameGraphResource<TResource>(string Name, TResource Descriptor) where TResource : notnull, IFrameGraphResource;

public class FrameGraphResources<TResource>
    where TResource : notnull, IFrameGraphResource
{
    TResource[] resources = new TResource[1];
    Dictionary<string, int> nameLookup = [];
    int count;

    public ResourceHandle<TResource> Add(TResource descriptor)
    {
        if (count == resources.Length) Array.Resize(ref resources, resources.Length * 2);
        resources[count] = descriptor;
        nameLookup[descriptor.Name] = count;
        return new ResourceHandle<TResource>(count++, descriptor.GetHashCode());
    }

    public ResourceHandle<TResource> GetHandle(string name)
    {
        if (nameLookup.TryGetValue(name, out int index))
        {
            return new ResourceHandle<TResource>(index, resources[index].GetHashCode());
        }
        return default;
    }
}