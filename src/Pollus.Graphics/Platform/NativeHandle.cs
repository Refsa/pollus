namespace Pollus.Graphics.Platform;

public readonly struct NativeHandle<TTag>
{
    public readonly nint Ptr;
    public NativeHandle(nint ptr)
    {
        Ptr = ptr;
    }
    public bool IsNull => Ptr == 0;

    unsafe public T* As<T>() where T : unmanaged
    {
        return (T*)Ptr;
    }

    public static NativeHandle<TTag> Null => new(nint.Zero);
}