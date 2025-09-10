namespace Pollus.ECS;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Read<C0> : ComponentWrapper<Read<C0>>.Target<C0>
    where C0 : unmanaged, IComponent
{
    static Read()
    {
        ComponentWrapper<Read<C0>>.Target<C0>.Init(new()
        {
            Read = true,
            Write = false,
        });
    }

    [FieldOffset(0)]
    public readonly C0 Component;
}
