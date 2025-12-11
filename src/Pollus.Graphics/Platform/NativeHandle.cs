namespace Pollus.Graphics.Platform;

public readonly struct NativeHandle<TResource>
{
    public readonly nint Ptr;

    public bool IsNull => Ptr == 0;

    public NativeHandle()
    {
        Ptr = nint.Zero;
    }
    
    public NativeHandle(nint ptr)
    {
        Ptr = ptr;
    }

    unsafe public T* As<T>() where T : unmanaged
    {
        return (T*)Ptr;
    }

    public static NativeHandle<TResource> Null => new(nint.Zero);

    public override int GetHashCode()
    {
        return Ptr.GetHashCode();
    }
}