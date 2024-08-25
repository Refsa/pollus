namespace Pollus.Engine.Input;

using System.Runtime.InteropServices;
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

    readonly Dictionary<Guid, IInputDevice> connectedDevices = [];
    readonly List<Mouse> mice = [];
    readonly List<Gamepad> gamepads = [];
    readonly Keyboard keyboard = new();

    bool isDisposed;

    public IEnumerable<IInputDevice> ConnectedDevices => connectedDevices.Values;

    ~InputManager() => Dispose();

    public InputManager()
    {
        connectedDevices.Add(keyboard.Id, keyboard);
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        foreach (var device in connectedDevices.Values)
        {
            device.Dispose();
        }
    }

    public void Update(PlatformEvents platform, Events events)
    {
        foreach (var @event in platform.Events)
        {
            HandleKeyboardEvent(@event);
            HandleMouseEvent(@event);
            if (!isBrowser) HandleGameControllerEvent(@event);
            HandleJoyDeviceEvent(@event);
        }

        foreach (var device in connectedDevices.Values)
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
            var mouse = mice.Find(e => e.ExternalId == @event.Motion.Which);
            if (mouse is null)
            {
                mouse = new Mouse((nint)@event.Motion.Which);
                AddDevice(mouse);
            }

            mouse.SetAxisState(MouseAxis.X, @event.Motion.Xrel);
            mouse.SetAxisState(MouseAxis.Y, @event.Motion.Yrel);
        }

        if (@event.Type is (int)Silk.NET.SDL.EventType.Mousebuttondown or (int)Silk.NET.SDL.EventType.Mousebuttonup)
        {
            var mouse = mice.Find(e => e.ExternalId == @event.Button.Which);
            if (mouse is null)
            {
                mouse = new Mouse((nint)@event.Button.Which);
                AddDevice(mouse);
            }

            var button = SDLMapping.MapMouseButton(@event.Button.Button);
            mouse.SetButtonState(button, @event.Button.State == 1);
        }
    }

    void HandleGameControllerEvent(Silk.NET.SDL.Event @event)
    {
        if (@event.Type is (int)Silk.NET.SDL.EventType.Controllerdeviceadded)
        {
            Gamepad? gamepad = gamepads.FirstOrDefault(e => e.ExternalId == @event.Cdevice.Which);
            if (gamepad is null)
            {
                gamepad = new Gamepad((nint)@event.Cdevice.Which);
                AddDevice(gamepad);
            }
            gamepad.Connect();
            Log.Info($"Gamepad connected: {gamepad.Id}");
        }

        if (@event.Type is (int)Silk.NET.SDL.EventType.Controllerdeviceremoved)
        {
            Gamepad? gamepad = gamepads.FirstOrDefault(e => e.ExternalId == @event.Cdevice.Which);
            gamepad?.Disconnect();
            Log.Info($"Gamepad disconnected: {gamepad?.Id}");
        }

        if (@event.Type is (int)Silk.NET.SDL.EventType.Controlleraxismotion)
        {
            var gamepad = gamepads.Find(e => e.ExternalId == @event.Caxis.Which);
            if (gamepad is null)
            {
                gamepad = new Gamepad((nint)@event.Caxis.Which);
                AddDevice(gamepad);
                gamepad.Connect();
            }

            var axis = SDLMapping.MapGamepadAxis((Silk.NET.SDL.GameControllerAxis)@event.Caxis.Axis);
            gamepad?.SetAxisState(axis, @event.Caxis.Value);
        }

        if (@event.Type is (int)Silk.NET.SDL.EventType.Controllerbuttondown or (int)Silk.NET.SDL.EventType.Controllerbuttonup)
        {
            var gamepad = gamepads.Find(e => e.ExternalId == @event.Cbutton.Which);
            if (gamepad is null)
            {
                gamepad = new Gamepad((nint)@event.Cbutton.Which);
                AddDevice(gamepad);
                gamepad.Connect();
            }

            var button = SDLMapping.MapGamepadButton((Silk.NET.SDL.GameControllerButton)@event.Cbutton.Button);
            gamepad?.SetButtonState(button, @event.Cbutton.State == 1);
        }
    }

    void HandleJoyDeviceEvent(Silk.NET.SDL.Event @event)
    {
        if (@event.Type is (int)Silk.NET.SDL.EventType.Joydeviceadded)
        {
            if (!isBrowser && Gamepad.IsGamepad(@event.Jdevice.Which)) return;

            var gamepad = gamepads.Find(e => e.ExternalId == @event.Jdevice.Which);
            if (gamepad is null)
            {
                gamepad = new Gamepad((nint)@event.Jdevice.Which);
                AddDevice(gamepad);
            }
            gamepad.Connect();
            Log.Info($"Joydevice connected: {gamepad.Id}");
        }

        if (@event.Type is (int)Silk.NET.SDL.EventType.Joydeviceremoved)
        {
            if (!isBrowser && Gamepad.IsGamepad(@event.Jdevice.Which)) return;

            var gamepad = gamepads.Find(e => e.ExternalId == @event.Jdevice.Which);
            gamepad?.Disconnect();
            Log.Info($"Joydevice disconnected: {gamepad?.Id}");
        }

        if (@event.Type is (int)Silk.NET.SDL.EventType.Joyaxismotion)
        {
            if (!isBrowser && Gamepad.IsGamepad(@event.Jdevice.Which)) return;

            var gamepad = gamepads.Find(e => e.ExternalId == @event.Jaxis.Which);
            if (gamepad is null)
            {
                gamepad = new Gamepad((nint)@event.Jaxis.Which);
                AddDevice(gamepad);
                gamepad.Connect();
            }

            var axis = SDLMapping.MapJoystickAxis(@event.Jaxis.Axis, gamepad.DeviceName);
            gamepad?.SetAxisState(axis, @event.Jaxis.Value);
        }

        if (@event.Type is (int)Silk.NET.SDL.EventType.Joybuttondown or (int)Silk.NET.SDL.EventType.Joybuttonup)
        {
            if (!isBrowser && Gamepad.IsGamepad(@event.Jdevice.Which)) return;

            var gamepad = gamepads.Find(e => e.ExternalId == @event.Jbutton.Which);
            if (gamepad is null)
            {
                gamepad = new Gamepad((nint)@event.Jbutton.Which);
                AddDevice(gamepad);
                gamepad.Connect();
            }
            
            var button = SDLMapping.MapJoystickButton(@event.Jbutton.Button, gamepad.DeviceName);
            gamepad?.SetButtonState(button, @event.Jbutton.State == 1);
        }
    }

    public IInputDevice? GetDevice(Guid id)
    {
        if (connectedDevices.TryGetValue(id, out var device))
        {
            return device;
        }
        return null;
    }

    protected void AddDevice<TDevice>(TDevice device)
        where TDevice : IInputDevice
    {
        connectedDevices.Add(device.Id, device);
        switch (device)
        {
            case Mouse mouse:
                mice.Add(mouse);
                break;
            case Gamepad gamepad:
                gamepads.Add(gamepad);
                break;
        }
    }

    protected void RemoveDevice(IInputDevice device)
    {
        connectedDevices.Remove(device.Id);
        switch (device.Type)
        {
            case InputType.Mouse:
                mice.Remove((Mouse)device);
                break;
            case InputType.Gamepad:
                gamepads.Remove((Gamepad)device);
                break;
        }
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