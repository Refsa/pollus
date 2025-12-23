namespace Pollus.Spatial;

using System.Runtime.CompilerServices;
using Pollus.ECS;

public partial struct Layer : IComponent
{
    public uint Value;

    public static Layer From<TLayer>(TLayer layer) where TLayer : unmanaged, Enum
    {
        return new Layer() { Value = Unsafe.As<TLayer, uint>(ref layer) };
    }
}