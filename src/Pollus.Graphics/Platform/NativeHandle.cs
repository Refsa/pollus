namespace Pollus.Graphics.Platform;

using System;
using System.Runtime.CompilerServices;

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
}