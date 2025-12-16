namespace Pollus.Engine.Input;

using Pollus.ECS;
using Pollus.Mathematics;

public class ButtonInput<TButton>
    where TButton : Enum
{
    static ButtonInput()
    {
        ResourceFetch<ButtonInput<TButton>>.Register();
    }

    Dictionary<Guid, IButtonInputDevice<TButton>> inputs = [];
    Guid primaryDevice = Guid.Empty;

    public void AddDevice(Guid id, IButtonInputDevice<TButton> device)
    {
        inputs[id] = device;
        if (primaryDevice == Guid.Empty) primaryDevice = id;
    }

    public void RemoveDevice(Guid id)
    {
        inputs.Remove(id);
        if (primaryDevice == id && inputs.Count > 0) primaryDevice = inputs.FirstOrDefault().Key;
    }

    IButtonInputDevice<TButton>? GetDevice(Guid device)
    {
        if (inputs.TryGetValue(device, out var input)) return input;
        return null;
    }

    public bool JustPressed(TButton button, Guid? device = null)
    {
        return GetDevice(device ?? primaryDevice)?.JustPressed(button) ?? false;
    }

    public bool JustReleased(TButton button, Guid? device = null)
    {
        return GetDevice(device ?? primaryDevice)?.JustReleased(button) ?? false;
    }

    public bool Pressed(TButton button, Guid? device = null)
    {
        return GetDevice(device ?? primaryDevice)?.Pressed(button) ?? false;
    }

    public float GetAxis(TButton negative, TButton positive, Guid? device = null)
    {
        float value = 0;
        if (Pressed(negative, device)) value -= 1;
        if (Pressed(positive, device)) value += 1;
        return value;
    }

    public Vec2f GetAxis2D(TButton negativeX, TButton positiveX, TButton negativeY, TButton positiveY, Guid? device = null)
    {
        return new(GetAxis(negativeX, positiveX, device), GetAxis(negativeY, positiveY, device));
    }
}
