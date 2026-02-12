namespace Pollus.Engine.Input;

public struct ButtonEvent<TButton>
    where TButton : Enum
{
    public required Guid DeviceId { get; init; }

    public required TButton Button { get; init; }
    public required ButtonState State { get; init; }

    public ButtonEvent(TButton button, ButtonState state)
    {
        Button = button;
        State = state;
    }
}

public struct TextInputEvent
{
    public required Guid DeviceId { get; init; }
    public required string Text { get; init; }
}

public struct AxisEvent<TAxis>
    where TAxis : Enum
{
    public required Guid DeviceId { get; init; }

    public required TAxis Axis { get; init; }
    public required float Value { get; init; }

    public AxisEvent(TAxis axis, float value)
    {
        Axis = axis;
        Value = value;
    }
}