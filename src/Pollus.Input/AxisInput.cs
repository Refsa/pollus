namespace Pollus.Input;

using Pollus.ECS;
using Pollus.Mathematics;

public class AxisInput<TAxis>
    where TAxis : Enum
{
    static AxisInput()
    {
        ResourceFetch<AxisInput<TAxis>>.Register();
    }

    Dictionary<Guid, IAxisInputDevice<TAxis>> inputs = [];
    Guid primaryDevice = Guid.Empty;

    public void AddDevice(Guid id, IAxisInputDevice<TAxis> device)
    {
        inputs[id] = device;
        if (primaryDevice == Guid.Empty) primaryDevice = id;
    }

    public void RemoveDevice(Guid id)
    {
        inputs.Remove(id);
        if (primaryDevice == id && inputs.Count > 0) primaryDevice = inputs.FirstOrDefault().Key;
    }

    IAxisInputDevice<TAxis>? GetDevice(Guid device)
    {
        if (inputs.Count == 0) return null;
        if (inputs.TryGetValue(device, out var input)) return input;
        return null;
    }

    public float GetAxis(TAxis axis, Guid? device = null)
    {
        return GetDevice(device ?? primaryDevice)?.GetAxis(axis) ?? 0;
    }

    public Vec2f GetAxis2D(TAxis xAxis, TAxis yAxis, Guid? device = null)
    {
        return new Vec2f(GetAxis(xAxis, device), GetAxis(yAxis, device));
    }
}
