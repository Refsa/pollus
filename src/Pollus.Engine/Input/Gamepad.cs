using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Graphics.SDL;

namespace Pollus.Engine.Input;

public enum GamepadButton
{
    Unknown = 0,
    None = 1,
    North,
    East,
    South,
    West,
    DPadLeft,
    DPadRight,
    DPadUp,
    DPadDown,
    LeftShoulder,
    RightShoulder,
    LeftStick,
    LeftTrigger,
    RightTrigger,
    RightStick,
    Start,
    Select,
    Back,
    Guide,
}

public enum GamepadAxis
{
    Unknown = 0,
    None = 1,
    LeftX,
    LeftY,
    LeftZ,
    RightX,
    RightY,
    RightZ,
}

public class Gamepad : IInputDevice, IAxisInputDevice<GamepadAxis>, IButtonInputDevice<GamepadButton>
{
    nint externalId;
    nint externalDevice;
    Guid id;

    Dictionary<GamepadButton, ButtonState> buttons = [];
    Dictionary<GamepadAxis, float> axes = [];
    HashSet<GamepadButton> changedButtons = [];
    HashSet<GamepadAxis> changedAxes = [];

    public nint ExternalId => externalId;
    public Guid Id => id;
    public InputType Type => InputType.Gamepad;

    public Gamepad(nint externalId)
    {
        id = Guid.NewGuid();
        this.externalId = externalId;
    }

    public void Dispose()
    {
        Disconnect();
    }

    public void Connect()
    {
        if (externalDevice != nint.Zero) return;

        unsafe
        {
#if BROWSER
            externalDevice = EmscriptenSDL.GameControllerOpen((int)externalId);
#else
            externalDevice = (nint)SDLWrapper.Instance.GameControllerOpen((int)externalId);
#endif
        }
    }

    public void Disconnect()
    {
        if (externalDevice == nint.Zero) return;

        unsafe
        {
#if BROWSER
            EmscriptenSDL.GameControllerClose(externalId);
#else
            SDLWrapper.Instance.GameControllerClose((Silk.NET.SDL.GameController*)externalDevice);
#endif
        }

        externalDevice = nint.Zero;
    }

    public void Update(Events events)
    {
        foreach (var key in buttons.Keys)
        {
            if (!changedButtons.Contains(key))
            {
                var prev = buttons[key];
                buttons[key] = buttons[key] switch
                {
                    ButtonState.JustPressed => ButtonState.Pressed,
                    ButtonState.JustReleased => ButtonState.None,
                    _ => buttons[key]
                };

                if (prev != buttons[key] && buttons[key] != ButtonState.None)
                {
                    changedButtons.Add(key);
                }
            }
        }

        var buttonEvents = events.GetWriter<ButtonEvent<GamepadButton>>();
        foreach (var key in changedButtons)
        {
            var state = buttons[key];
            if (state is not ButtonState.JustPressed or ButtonState.JustReleased) continue;

            buttonEvents.Write(new ButtonEvent<GamepadButton>
            {
                DeviceId = Id,
                Button = key,
                State = state,
            });
        }

        var axisEvents = events.GetWriter<AxisEvent<GamepadAxis>>();
        foreach (var axis in changedAxes)
        {
            axisEvents.Write(new AxisEvent<GamepadAxis>
            {
                DeviceId = Id,
                Axis = axis,
                Value = axes[axis],
            });
        }

        changedButtons.Clear();
        changedAxes.Clear();
    }

    public float GetAxis(GamepadAxis axis)
    {
        return axes.TryGetValue(axis, out var value) ? value : 0;
    }

    public bool JustPressed(GamepadButton button)
    {
        return GetButtonState(button) is ButtonState.JustPressed;
    }

    public bool JustReleased(GamepadButton button)
    {
        return GetButtonState(button) is ButtonState.JustReleased;
    }

    public bool Pressed(GamepadButton button)
    {
        return GetButtonState(button) is ButtonState.Pressed;
    }

    public ButtonState GetButtonState(GamepadButton button)
    {
        return buttons.TryGetValue(button, out var state) ? state : ButtonState.None;
    }

    public void SetButtonState(GamepadButton button, bool isPressed)
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

    public void SetAxisState(GamepadAxis axis, float value)
    {
        changedAxes.Add(axis);
        axes[axis] = value;
    }

    public static bool IsGamepad(int index)
    {
#if BROWSER
        return EmscriptenSDL.IsGameController(index);
#else
        return SDLWrapper.Instance.IsGameController(index) != 0;
#endif
    }
}