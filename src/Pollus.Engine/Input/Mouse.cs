using Pollus.ECS;

namespace Pollus.Engine.Input;

public enum MouseButton
{
    Unknown = 0,
    None = 1,
    Left,
    Right,
    Middle,
    Button4,
    Button5,
    Button6,
    Button7,
    Button8,
}

public enum MouseAxis
{
    Unknown = 0,
    None = 1,
    X,
    Y,
    ScrollX,
    ScrollY,
}

public class Mouse : IInputDevice, IAxisInputDevice<MouseAxis>, IButtonInputDevice<MouseButton>
{
    Guid id;
    nint externalId;

    Dictionary<MouseButton, ButtonState> buttons = new();
    Dictionary<MouseAxis, float> axes = new();
    HashSet<MouseButton> changedButtons = new();
    HashSet<MouseAxis> changedAxes = new();

    public nint ExternalId => externalId;
    public Guid Id => id;
    public InputType Type => InputType.Mouse;

    public Mouse(nint externalId)
    {
        id = Guid.NewGuid();
        this.externalId = externalId;
    }

    public void Dispose()
    {

    }

    public void Update(Events events)
    {
        foreach (var key in buttons.Keys)
        {
            if (!changedButtons.Contains(key))
            {
                buttons[key] = buttons[key] switch
                {
                    ButtonState.JustPressed => ButtonState.Pressed,
                    ButtonState.JustReleased => ButtonState.None,
                    _ => buttons[key]
                };
            }
        }

        var buttonEvents = events.GetWriter<ButtonEvent<MouseButton>>();
        foreach (var button in changedButtons)
        {
            var state = buttons[button];
            if (state is ButtonState.JustPressed or ButtonState.JustReleased)
            {
                buttonEvents.Write(new ButtonEvent<MouseButton>()
                {
                    Button = button,
                    State = state,
                    DeviceId = id,
                });
            }
        }

        var axesEvents = events.GetWriter<AxisEvent<MouseAxis>>();
        foreach (var axis in changedAxes)
        {
            axesEvents.Write(new AxisEvent<MouseAxis>()
            {
                Axis = axis,
                Value = axes[axis],
                DeviceId = id,
            });
        }

        changedButtons.Clear();
        changedAxes.Clear();
    }

    public void SetButtonState(MouseButton button, bool isPressed)
    {
        var state = GetButtonState(button);
        if (isPressed)
        {
            if (state == ButtonState.None || state == ButtonState.JustReleased)
            {
                state = ButtonState.JustPressed;
            }
            else if (state == ButtonState.JustPressed)
            {
                state = ButtonState.Pressed;
            }
        }
        else if (state == ButtonState.Pressed || state == ButtonState.JustPressed)
        {
            state = ButtonState.JustReleased;
        }

        changedButtons.Add(button);
        buttons[button] = state;
    }

    public ButtonState GetButtonState(MouseButton button)
    {
        return buttons.TryGetValue(button, out var state) ? state : ButtonState.None;
    }

    public void SetAxisState(MouseAxis axis, float value)
    {
        axes[axis] = value;
        changedAxes.Add(axis);
    }

    public float GetAxis(MouseAxis axis)
    {
        return axes.TryGetValue(axis, out var value) ? value : 0;
    }

    public bool JustPressed(MouseButton button)
    {
        return GetButtonState(button) is ButtonState.JustPressed;
    }

    public bool Pressed(MouseButton button)
    {
        return GetButtonState(button) is ButtonState.Pressed or ButtonState.JustPressed;
    }

    public bool JustReleased(MouseButton button)
    {
        return GetButtonState(button) is ButtonState.JustReleased;
    }
}