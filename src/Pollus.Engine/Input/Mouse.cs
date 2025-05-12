using Pollus.ECS;
using Pollus.Mathematics;

namespace Pollus.Engine.Input;

public enum MouseButton
{
    Unknown = -1,
    None = 0,
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
    DeltaX,
    DeltaY,
    ScrollX,
    ScrollY,
}

public struct MouseMovedEvent
{
    public Vec2<int> Position;
    public Vec2<int> Delta;
}

public class Mouse : IInputDevice, IAxisInputDevice<MouseAxis>, IButtonInputDevice<MouseButton>
{
    Guid id;
    nint externalId;

    Vec2<int> position;
    Vec2<int> delta;
    Dictionary<MouseButton, ButtonState> buttons = new();
    Dictionary<MouseAxis, float> axes = new();

    HashSet<MouseButton> changedButtons = new();
    HashSet<MouseAxis> changedAxes = new();
    bool positionChanged;

    public nint ExternalId => externalId;
    public Guid Id => id;
    public InputType Type => InputType.Mouse;
    public Vec2<int> Position => position;
    public Vec2<int> Delta => delta;

    public Mouse(nint externalId)
    {
        id = Guid.NewGuid();
        this.externalId = externalId;
    }

    public void Dispose()
    {

    }

    public void PreUpdate()
    {
        delta = Vec2<int>.Zero;
        SetAxisState(MouseAxis.DeltaX, 0, false);
        SetAxisState(MouseAxis.DeltaY, 0, false);
        SetAxisState(MouseAxis.ScrollX, 0, false);
        SetAxisState(MouseAxis.ScrollY, 0, false);
        SetAxisState(MouseAxis.X, 0, false);
        SetAxisState(MouseAxis.Y, 0, false);
    }

    public void Update(Events events)
    {
        foreach (var key in buttons.Keys)
        {
            if (changedButtons.Contains(key)) continue;

            buttons[key] = buttons[key] switch
            {
                ButtonState.JustPressed => ButtonState.Pressed,
                ButtonState.JustReleased => ButtonState.None,
                _ => buttons[key]
            };
        }

        var buttonEvents = events.GetWriter<ButtonEvent<MouseButton>>();
        foreach (var button in changedButtons)
        {
            var state = buttons[button];
            if (state is not (ButtonState.JustPressed or ButtonState.JustReleased)) continue;

            buttonEvents.Write(new ButtonEvent<MouseButton>()
            {
                Button = button,
                State = state,
                DeviceId = id,
            });
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

        if (positionChanged)
        {
            var movedEvent = new MouseMovedEvent()
            {
                Position = position,
                Delta = delta,
            };

            events.GetWriter<MouseMovedEvent>().Write(movedEvent);
        }

        positionChanged = false;
        changedButtons.Clear();
        changedAxes.Clear();
    }

    public void SetPosition(int x, int y)
    {
        positionChanged = position.X != x || position.Y != y;
        var next = new Vec2<int>(x, y);
        delta = positionChanged ? next - position : Vec2<int>.Zero;
        SetAxisState(MouseAxis.DeltaX, delta.X);
        SetAxisState(MouseAxis.DeltaY, delta.Y);
        position = next;
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

    public void SetAxisState(MouseAxis axis, float value, bool flagChanged = true)
    {
        axes[axis] = value;
        if (flagChanged) changedAxes.Add(axis);
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