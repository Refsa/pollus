using System.Runtime.InteropServices;

namespace Pollus.ECS;

public class ComponentColumn
{
    byte[] data;
    public int Size { get; init; }
    public ComponentID ComponentID { get; init; }

    public static ComponentColumn From(ComponentID cid)
    {
        var info = Component.GetInfo(cid);
        return new()
        {
            Size = info.Size,
            data = new byte[256 * info.Size],
            ComponentID = cid,
        };
    }

    public static ComponentColumn From<C>() where C : unmanaged, IComponent
    {
        var info = Component.GetInfo<C>();
        return new()
        {
            Size = info.Size,
            data = new byte[256 * info.Size],
            ComponentID = info.ID,
        };
    }

    public void Set<C>(int row, C value) where C : unmanaged, IComponent
    {
        var offset = row * Size;
        var span = MemoryMarshal.Cast<byte, C>(data.AsSpan(offset, Size));
        span[0] = value;
    }

    public ref C Get<C>(int row) where C : unmanaged, IComponent
    {
        var offset = row * Size;
        var span = MemoryMarshal.Cast<byte, C>(data.AsSpan(offset, Size));
        return ref span[0];
    }
}