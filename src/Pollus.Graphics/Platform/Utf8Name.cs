namespace Pollus.Graphics.Platform;

using System;

public readonly struct Utf8Name
{
    public readonly nint Pointer;
    public Utf8Name(nint pointer)
    {
        Pointer = pointer;
    }
    public static implicit operator nint(Utf8Name name) => name.Pointer;
}


