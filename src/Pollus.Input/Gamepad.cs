namespace Pollus.Input;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Mathematics;
using Silk.NET.SDL;

public class Gamepad : IInputDevice, IAxisInputDevice<GamepadAxis>, IButtonInputDevice<GamepadButton>
{
    nint externalId;
    nint externalDevice;
    string? deviceName;
    Guid id;
    bool isActive;

    Dictionary<GamepadButton, ButtonState> buttons = [];
    Dictionary<GamepadAxis, float> axes = [];
    HashSet<GamepadButton> changedButtons = [];
    HashSet<GamepadAxis> changedAxes = [];

    public string DeviceName => deviceName ?? "Unknown Gamepad Device Name";
    public nint ExternalId => externalId;
    public Guid Id => id;
    public InputType Type => InputType.Gamepad;
    public bool IsActive => true;

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
            if (OperatingSystem.IsBrowser())
            {
                externalDevice = EmscriptenSDL.JoystickOpen((int)externalId);
                deviceName = EmscriptenSDL.JoystickName((int)externalId);
            }
            else
            {
                var sdl = SdlProvider.SDL.Value;
                externalDevice = (nint)sdl.GameControllerOpen((int)externalId);
                deviceName = sdl.GameControllerNameS((GameController*)externalDevice);
            }
        }

        Guard.IsNotNull(externalDevice, "Failed to open gamepad device");
    }

    public void Disconnect()
    {
        if (externalDevice == nint.Zero) return;

        unsafe
        {
            if (OperatingSystem.IsBrowser())
            {
                EmscriptenSDL.JoystickClose((int)externalId);
            }
            else
            {
                SdlProvider.SDL.Value.GameControllerClose((GameController*)externalDevice);
            }
        }

        externalDevice = nint.Zero;
    }

    public void PreUpdate()
    {

    }

    public void Update(Events events)
    {
        foreach (var key in buttons.Keys)
        {
            if (changedButtons.Contains(key)) continue;

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

        var buttonEvents = events.GetWriter<ButtonEvent<GamepadButton>>();
        foreach (var key in changedButtons)
        {
            var state = buttons[key];
            if (state is not (ButtonState.JustPressed or ButtonState.JustReleased)) continue;

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

        isActive = changedButtons.Count > 0 || changedAxes.Count > 0;
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

    public void SetAxisState(GamepadAxis axis, short value)
    {
        var nvalue = Math.Abs((float)value / short.MaxValue);
        nvalue = Math.Clamp(nvalue, 0f, 1f);
        if (nvalue < 0.02) nvalue = 0;
        nvalue /= Math.Sign(value);

        if (axes.TryGetValue(axis, out var prev) && prev != nvalue)
        {
            changedAxes.Add(axis);
        }
        axes[axis] = nvalue;
    }

    public void SetAxisState(GamepadAxis axis, float value)
    {
        if (axes.TryGetValue(axis, out var prev) && prev != value)
        {
            changedAxes.Add(axis);
        }
        axes[axis] = value;
    }

    public static bool IsGamepad(int index)
    {
        if (OperatingSystem.IsBrowser()) return EmscriptenSDL.IsGameController(index);
        return SdlProvider.SDL.Value.IsGameController(index) != 0;
    }
}
