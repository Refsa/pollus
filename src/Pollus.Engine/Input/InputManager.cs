namespace Pollus.Engine.Input;

using System.Runtime.InteropServices;
using Pollus.Collections;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Engine.Platform;

public enum InputType
{
    Keyboard,
    Mouse,
    Gamepad,
    Touch,
}

public class InputManager : IDisposable
{
    static readonly bool isBrowser = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));

    readonly List<IInputDevice> devices = [];
    readonly List<Mouse> mice = [];
    readonly List<Gamepad> gamepads = [];
    readonly Keyboard keyboard;

    bool isDisposed;

    public ListEnumerable<IInputDevice> Devices => new(devices);

    ~InputManager() => Dispose();

    public InputManager()
    {
        keyboard = new();
        devices.Add(keyboard);
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        foreach (var device in devices)
        {
            device.Dispose();
        }
    }

    public void Update(PlatformEvents platform, Events events)
    {
        foreach (var device in devices)
        {
            device.PreUpdate();
        }

        foreach (var @event in platform.Events)
        {
            HandleKeyboardEvent(@event);
            HandleMouseEvent(@event);
            if (!isBrowser) HandleGameControllerEvent(@event);
            HandleJoyDeviceEvent(@event);
        }

        foreach (var device in devices)
        {
            device.Update(events);
        }
    }

    void HandleKeyboardEvent(Silk.NET.SDL.Event @event)
    {
        if (@event.Type is (int)Silk.NET.SDL.EventType.Keydown or (int)Silk.NET.SDL.EventType.Keyup)
        {
            var key = SDLMapping.MapKey(@event.Key.Keysym.Scancode);
            if (@event.Key.Repeat == 0)
            {
                keyboard?.SetKeyState(key, @event.Key.State == 1);
            }
        }
    }

    void HandleMouseEvent(Silk.NET.SDL.Event @event)
    {
        if (@event.Type is (int)Silk.NET.SDL.EventType.Mousemotion)
        {
            var mouse = FindDeviceByExternalID<Mouse>((nint)@event.Button.Which)
                ?? AddDevice(new Mouse((nint)@event.Button.Which));

            mouse.SetAxisState(MouseAxis.X, @event.Motion.Xrel);
            mouse.SetAxisState(MouseAxis.Y, @event.Motion.Yrel);
            mouse.SetPosition(@event.Motion.X, @event.Motion.Y);
        }
        else if (@event.Type is (int)Silk.NET.SDL.EventType.Mousewheel)
        {
            var mouse = FindDeviceByExternalID<Mouse>((nint)@event.Button.Which)
                ?? AddDevice(new Mouse((nint)@event.Button.Which));

            mouse.SetAxisState(MouseAxis.ScrollX, @event.Wheel.X);
            mouse.SetAxisState(MouseAxis.ScrollY, @event.Wheel.Y);
            mouse.SetPosition(@event.Wheel.MouseX, @event.Wheel.MouseY);
        }
        else if (@event.Type is (int)Silk.NET.SDL.EventType.Mousebuttondown or (int)Silk.NET.SDL.EventType.Mousebuttonup)
        {
            var mouse = FindDeviceByExternalID<Mouse>((nint)@event.Button.Which)
                ?? AddDevice(new Mouse((nint)@event.Button.Which));

            var button = SDLMapping.MapMouseButton(@event.Button.Button);
            mouse.SetButtonState(button, @event.Button.State == 1);
            mouse.SetPosition(@event.Button.X, @event.Button.Y);
        }
    }

    void HandleGameControllerEvent(Silk.NET.SDL.Event @event)
    {
        if (@event.Type is (int)Silk.NET.SDL.EventType.Controllerdeviceadded)
        {
            var gamepad = FindDeviceByExternalID<Gamepad>((nint)@event.Cdevice.Which)
                ?? AddDevice(new Gamepad((nint)@event.Cdevice.Which));

            gamepad.Connect();
            Log.Info($"Gamepad connected: {gamepad.Id}");
        }
        else if (@event.Type is (int)Silk.NET.SDL.EventType.Controllerdeviceremoved)
        {
            var gamepad = FindDeviceByExternalID<Gamepad>((nint)@event.Cdevice.Which);

            gamepad?.Disconnect();
            Log.Info($"Gamepad disconnected: {gamepad?.Id}");
        }
        else if (@event.Type is (int)Silk.NET.SDL.EventType.Controlleraxismotion)
        {
            var gamepad = FindDeviceByExternalID<Gamepad>((nint)@event.Cdevice.Which)
                ?? AddDevice(new Gamepad((nint)@event.Cdevice.Which));

            var axis = SDLMapping.MapGamepadAxis((Silk.NET.SDL.GameControllerAxis)@event.Caxis.Axis);
            gamepad?.SetAxisState(axis, @event.Caxis.Value);
        }
        else if (@event.Type is (int)Silk.NET.SDL.EventType.Controllerbuttondown or (int)Silk.NET.SDL.EventType.Controllerbuttonup)
        {
            var gamepad = FindDeviceByExternalID<Gamepad>((nint)@event.Cdevice.Which)
                ?? AddDevice(new Gamepad((nint)@event.Cdevice.Which));

            var button = SDLMapping.MapGamepadButton((Silk.NET.SDL.GameControllerButton)@event.Cbutton.Button);
            gamepad?.SetButtonState(button, @event.Cbutton.State == 1);
        }
    }

    void HandleJoyDeviceEvent(Silk.NET.SDL.Event @event)
    {
        if (@event.Type is (int)Silk.NET.SDL.EventType.Joydeviceadded)
        {
            if (!isBrowser && Gamepad.IsGamepad(@event.Jdevice.Which)) return;

            var gamepad = FindDeviceByExternalID<Gamepad>((nint)@event.Cdevice.Which)
                ?? AddDevice(new Gamepad((nint)@event.Cdevice.Which));

            gamepad.Connect();
            Log.Info($"Joydevice connected: {gamepad.Id}");
        }
        else if (@event.Type is (int)Silk.NET.SDL.EventType.Joydeviceremoved)
        {
            if (!isBrowser && Gamepad.IsGamepad(@event.Jdevice.Which)) return;

            var gamepad = FindDeviceByExternalID<Gamepad>((nint)@event.Cdevice.Which);
            gamepad?.Disconnect();
            Log.Info($"Joydevice disconnected: {gamepad?.Id}");
        }
        else if (@event.Type is (int)Silk.NET.SDL.EventType.Joyaxismotion)
        {
            if (!isBrowser && Gamepad.IsGamepad(@event.Jdevice.Which)) return;

            var gamepad = FindDeviceByExternalID<Gamepad>((nint)@event.Cdevice.Which)
                ?? AddDevice(new Gamepad((nint)@event.Cdevice.Which));

            var axis = SDLMapping.MapJoystickAxis(@event.Jaxis.Axis, gamepad.DeviceName);
            gamepad.SetAxisState(axis, @event.Jaxis.Value);
        }
        else if (@event.Type is (int)Silk.NET.SDL.EventType.Joybuttondown or (int)Silk.NET.SDL.EventType.Joybuttonup)
        {
            if (!isBrowser && Gamepad.IsGamepad(@event.Jdevice.Which)) return;

            var gamepad = FindDeviceByExternalID<Gamepad>((nint)@event.Cdevice.Which)
                ?? AddDevice(new Gamepad((nint)@event.Cdevice.Which));

            var button = SDLMapping.MapJoystickButton(@event.Jbutton.Button, gamepad.DeviceName);
            gamepad.SetButtonState(button, @event.Jbutton.State == 1);
        }
    }

    public IInputDevice? GetDevice(Guid id)
    {
        return FindDeviceByID(id);
    }

    protected TDevice AddDevice<TDevice>(TDevice device)
        where TDevice : IInputDevice
    {
        devices.Add(device);
        if (device is Mouse mouse)
        {
            mice.Add(mouse);
        }
        else if (device is Gamepad gamepad)
        {
            gamepads.Add(gamepad);
        }

        return device;
    }

    protected void RemoveDevice(IInputDevice device)
    {
        devices.Remove(device);
        if (device is Mouse mouse)
        {
            mice.Remove(mouse);
        }
        else if (device is Gamepad gamepad)
        {
            gamepads.Remove(gamepad);
        }
    }

    protected IInputDevice? FindDeviceByID(Guid id)
    {
        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i].Id == id) return devices[i];
        }
        return null;
    }

    protected TDevice? FindDeviceByExternalID<TDevice>(nint externalId)
        where TDevice : class, IInputDevice
    {
        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i] is TDevice device && device.ExternalId == externalId)
            {
                return device;
            }
        }
        return null;
    }

    /// <summary>
    /// get device by path:
    /// keyboard/0
    /// keyboard
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IInputDevice? GetDevice(ReadOnlySpan<char> path)
    {
        Span<Range> ranges = stackalloc Range[2];
        int count = path.Split(ranges, '/', StringSplitOptions.RemoveEmptyEntries);
        if (count == 0) return null;

        if (!int.TryParse(path[ranges[1]], out var index))
        {
            index = 0;
        }

        return path[ranges[0]] switch
        {
            "keyboard" => keyboard,
            "mouse" => index >= mice.Count ? null : mice[index],
            "gamepad" => index >= gamepads.Count ? null : gamepads[index],
            _ => throw new NotImplementedException(),
        };
    }

    public TDevice? GetDevice<TDevice>(ReadOnlySpan<char> path)
        where TDevice : class, IInputDevice
    {
        return GetDevice(path) as TDevice;
    }
}