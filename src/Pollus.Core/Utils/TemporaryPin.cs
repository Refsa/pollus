namespace Pollus.Utils;

using System.Runtime.InteropServices;

public ref struct TemporaryPin
{
    GCHandle handle;

    public GCHandle Handle => handle;

    public nint Ptr => (nint)handle.AddrOfPinnedObject();

    public TemporaryPin(GCHandle handle)
    {
        this.handle = handle;
    }

    public static TemporaryPin Pin<T>(T obj)
    {
        return new TemporaryPin(GCHandle.Alloc(obj, GCHandleType.Pinned));
    }

    public static TemporaryPin PinString(string str)
    {
        return new TemporaryPin(GCHandle.Alloc(System.Text.Encoding.UTF8.GetBytes(str), GCHandleType.Pinned));
    }

    public void Dispose()
    {
        handle.Free();
    }
}

public ref struct TemporaryPins
{
    List<GCHandle> pins;

    public TemporaryPins()
    {
        pins = new();
    }

    public GCHandle Pin<T>(T obj)
    {
        var handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
        pins.Add(handle);
        return handle;
    }

    public GCHandle PinString(string str)
    {
        var handle = GCHandle.Alloc(System.Text.Encoding.UTF8.GetBytes(str), GCHandleType.Pinned);
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