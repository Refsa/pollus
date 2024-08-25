namespace Pollus.Engine.Input;

using Pollus.ECS;
using Pollus.Mathematics;

public interface IInputDevice : IDisposable
{
    nint ExternalId { get; }
    Guid Id { get; }
    InputType Type { get; }

    void Update(Events events);
}

public enum ButtonState
{
    None = 0,
    JustPressed = 1,
    Pressed = 2,
    JustReleased = 3,
}

public interface IButtonInputDevice<TButton>
    where TButton : Enum
{
    bool JustPressed(TButton button);
    bool Pressed(TButton button);
    bool JustReleased(TButton button);
}

public interface IAxisInputDevice<TAxis>
    where TAxis : Enum
{
    float GetAxis(TAxis axis);
    Vec2f GetAxis2D(TAxis xAxis, TAxis yAxis) => new(GetAxis(xAxis), GetAxis(yAxis));
}

public class ButtonInput<TButton>
    where TButton : Enum
{
    Dictionary<Guid, IButtonInputDevice<TButton>> inputs = [];

    public void AddDevice(Guid id, IButtonInputDevice<TButton> device)
    {
        inputs[id] = device;
    }

    public void RemoveDevice(Guid id)
    {
        inputs.Remove(id);
    }

    public bool JustPressed(TButton button, Guid? device = null)
    {
        if (inputs.Count == 0) return false;
        if (device != null && !inputs.TryGetValue(device.Value, out var input)) return false;
        return inputs.Values.Any(input => input.JustPressed(button));
    }

    public bool JustReleased(TButton button, Guid? device = null)
    {
        if (inputs.Count == 0) return false;
        if (device != null && !inputs.TryGetValue(device.Value, out var input)) return false;
        return inputs.Values.Any(input => input.JustReleased(button));
    }

    public bool Pressed(TButton button, Guid? device = null)
    {
        if (inputs.Count == 0) return false;
        if (device != null && !inputs.TryGetValue(device.Value, out var input)) return false;
        return inputs.Values.Any(input => input.Pressed(button));
    }

    public float GetAxis(TButton negative, TButton positive, Guid? device = null)
    {
        float value = 0;
        if (Pressed(positive, device)) value += 1;
        if (Pressed(negative, device)) value -= 1;
        return value;
    }

    public Vec2f GetAxis2D(TButton negativeX, TButton positiveX, TButton negativeY, TButton positiveY, Guid? device = null)
    {
        return new(GetAxis(positiveX, negativeX, device), GetAxis(positiveY, negativeY, device));
    }
}

public class AxisInput<TAxis>
    where TAxis : Enum
{
    Dictionary<Guid, IAxisInputDevice<TAxis>> inputs = [];

    public void AddDevice(Guid id, IAxisInputDevice<TAxis> device)
    {
        inputs[id] = device;
    }

    public void RemoveDevice(Guid id)
    {
        inputs.Remove(id);
    }

    public float GetAxis(TAxis axis, Guid? device = null)
    {
        if (inputs.Count == 0) return 0;
        if (device != null && !inputs.TryGetValue(device.Value, out var input)) return 0;
        return inputs.Values.Sum(input => input.GetAxis(axis));
    }

    public Vec2f GetAxis2D(TAxis xAxis, TAxis yAxis, Guid? device = null)
    {
        return new Vec2f(GetAxis(xAxis, device), GetAxis(yAxis, device));
    }
}