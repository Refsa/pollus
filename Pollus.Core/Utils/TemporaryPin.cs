namespace Pollus.Utils;

using System.Runtime.InteropServices;

public ref struct TemporaryPins
{
    List<GCHandle> pins;

    public TemporaryPins()
    {
        pins = new();
    }

    public GCHandle Add(GCHandle handle)
    {
        pins.Add(handle);
        return handle;
    }

    public GCHandle Pin<T>(T obj)
    {
        var handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
        pins.Add(handle);
        return handle;
    }

    public void Dispose()
    {
        foreach (var pin in pins)
        {
            pin.Free();
        }
    }
}