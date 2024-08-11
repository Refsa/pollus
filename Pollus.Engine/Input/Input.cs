using System.Data;
using System.Runtime.InteropServices;
using Pollus.Mathematics;

namespace Pollus.Engine.Input;

public enum InputType
{
    Keyboard,
    Mouse,
    Gamepad,
    Touch,
}

public abstract class InputManager : IDisposable
{
    Dictionary<Guid, IInputDevice> connectedDevices = new();
    List<IInputDevice> keyboards = new();
    List<IInputDevice> mice = new();
    List<IInputDevice> gamepads = new();

    bool isDisposed;

    public IEnumerable<IInputDevice> ConnectedDevices => connectedDevices.Values;

    ~InputManager() => Dispose();

    public void Update()
    {
        UpdateInternal();
        foreach (var device in connectedDevices.Values)
        {
            device.Update();
        }
    }

    protected abstract void UpdateInternal();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        foreach (var device in connectedDevices.Values)
        {
            device.Dispose();
        }

        Dispose(true);
    }

    protected abstract void Dispose(bool disposing);

    public IInputDevice? GetDevice(Guid id)
    {
        if (connectedDevices.TryGetValue(id, out var device))
        {
            return device;
        }
        return null;
    }

    protected void AddDevice(IInputDevice device)
    {
        connectedDevices.Add(device.Id, device);
        device.Index = connectedDevices.Count - 1;
        switch (device.Type)
        {
            case InputType.Keyboard:
                keyboards.Add(device);
                break;
            case InputType.Mouse:
                mice.Add(device);
                break;
            case InputType.Gamepad:
                gamepads.Add(device);
                break;
        }
    }

    protected void RemoveDevice(IInputDevice device)
    {
        connectedDevices.Remove(device.Id);
        switch (device.Type)
        {
            case InputType.Keyboard:
                keyboards.Remove(device);
                break;
            case InputType.Mouse:
                mice.Remove(device);
                break;
            case InputType.Gamepad:
                gamepads.Remove(device);
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

        var devices = path[ranges[0]] switch
        {
            "keyboard" => keyboards,
            "mouse" => mice,
            "gamepad" => gamepads,
            _ => throw new NotImplementedException(),
        };

        if (int.TryParse(path[ranges[1]], out var index))
        {
            return devices.FirstOrDefault(d => d.Index == index);
        }

        return devices.FirstOrDefault();
    }

    public TDevice? GetDevice<TDevice>(ReadOnlySpan<char> path)
        where TDevice : class, IInputDevice
    {
        return GetDevice(path) as TDevice;
    }

    public static InputManager Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
        {
            return new BrowserInput();
        }
        return new DesktopInput();
    }
}

public interface IInputDevice : IDisposable
{
    nint ExternalId { get; }
    Guid Id { get; }
    int Index { get; internal set; }
    InputType Type { get; }

    void Update();
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
    Vector2<float> GetAxis2D(TAxis xAxis, TAxis yAxis) => new(GetAxis(xAxis), GetAxis(yAxis));
}

public interface IInputDeviceEvent
{
    InputType InputType { get; }
}

public enum ButtonState
{
    None = 0,
    JustPressed = 1,
    Pressed = 2,
    JustReleased = 3,
}

public interface IButtonEvent : IInputDeviceEvent
{
    ButtonState State { get; }
}

public interface IAxisEvent : IInputDeviceEvent
{
    float Value { get; }
}

public struct DeviceConnected : IInputDeviceEvent
{
    public InputType InputType => throw new NotImplementedException();
}

public struct DeviceDisconnected : IInputDeviceEvent
{
    public InputType InputType => throw new NotImplementedException();
}