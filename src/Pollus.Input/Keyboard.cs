using Pollus.ECS;
using Pollus.Mathematics;

namespace Pollus.Input;

public enum Key
{
    Unknown = 0,
    None = 1,
    ArrowLeft,
    ArrowRight,
    ArrowDown,
    ArrowUp,
    Backspace,
    Tab,
    Enter,
    LeftShift,
    RightShift,
    LeftControl,
    RightControl,
    LeftAlt,
    RightAlt,
    LeftMeta,
    RightMeta,
    CapsLock,
    Escape,
    Space,
    PageUp,
    PageDown,
    End,
    Home,
    Insert,
    Pause,
    Equal,
    ScrollLock,
    PrintScreen,
    Delete,
    LeftBracket,
    RightBracket,
    Backslash,
    NonUSHash,
    Semicolon,
    Apostrophe,
    Grave,
    Comma,
    Period,
    Slash,
    Minus,
    Digit0 = 48,
    Digit1 = 49,
    Digit2 = 50,
    Digit3 = 51,
    Digit4 = 52,
    Digit5 = 53,
    Digit6 = 54,
    Digit7 = 55,
    Digit8 = 56,
    Digit9 = 57,
    KeyA = 65,
    KeyB = 66,
    KeyC = 67,
    KeyD = 68,
    KeyE = 69,
    KeyF = 70,
    KeyG = 71,
    KeyH = 72,
    KeyI = 73,
    KeyJ = 74,
    KeyK = 75,
    KeyL = 76,
    KeyM = 77,
    KeyN = 78,
    KeyO = 79,
    KeyP = 80,
    KeyQ = 81,
    KeyR = 82,
    KeyS = 83,
    KeyT = 84,
    KeyU = 85,
    KeyV = 86,
    KeyW = 87,
    KeyX = 88,
    KeyY = 89,
    KeyZ = 90,
    F1,
    F2,
    F3,
    F4,
    F5,
    F6,
    F7,
    F8,
    F9,
    F10,
    F11,
    F12,
}

[Flags]
public enum ModifierKey
{
    None = 0,
    LeftShift = 0x0001,
    RightShift = 0x0002,
    LeftControl = 0x0040,
    RightControl = 0x0080,
    LeftAlt = 0x0100,
    RightAlt = 0x0200,
    LeftMeta = 0x0400,
    RightMeta = 0x0800,
    NumLock = 0x1000,
    CapsLock = 0x2000,
    Mode = 0x4000,
    Control = LeftControl | RightControl,
    Shift = LeftShift | RightShift,
    Alt = LeftAlt | RightAlt,
    Meta = LeftMeta | RightMeta,
}

public class Keyboard : IInputDevice, IButtonInputDevice<Key>
{
    public nint ExternalId { get; }
    public Guid Id { get; } = new();
    public InputType Type => InputType.Keyboard;
    public bool IsActive => true;

    Dictionary<Key, ButtonState> buttons = new();
    HashSet<Key> changed = new();
    List<string> pendingTextInput = new();
    bool isActive;

    public void EnqueueTextInput(string text)
    {
        pendingTextInput.Add(text);
    }

    public void SetKeyState(Key key, bool isPressed)
    {
        var state = GetKeyState(key);
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

        buttons[key] = state;
        changed.Add(key);
    }

    public void PreUpdate()
    {

    }

    public void Update(Events events)
    {
        foreach (var key in buttons.Keys)
        {
            if (changed.Contains(key)) continue;

            var prev = buttons[key];
            buttons[key] = buttons[key] switch
            {
                ButtonState.JustPressed => ButtonState.Pressed,
                ButtonState.JustReleased => ButtonState.None,
                _ => buttons[key]
            };

            if (prev != buttons[key] && buttons[key] != ButtonState.None)
            {
                changed.Add(key);
            }
        }

        var keyEvents = events.GetWriter<ButtonEvent<Key>>();
        foreach (var key in changed)
        {
            var state = buttons[key];
            if (state is not (ButtonState.JustPressed or ButtonState.JustReleased)) continue;

            keyEvents.Write(new ButtonEvent<Key>
            {
                DeviceId = Id,
                Button = key,
                State = state,
            });
        }

        var textEvents = events.GetWriter<TextInputEvent>();
        foreach (var text in pendingTextInput)
        {
            textEvents.Write(new TextInputEvent
            {
                DeviceId = Id,
                Text = text,
            });
        }

        isActive = changed.Count > 0 || pendingTextInput.Count > 0;
        changed.Clear();
        pendingTextInput.Clear();
    }

    public ButtonState GetKeyState(Key key)
    {
        return buttons.TryGetValue(key, out var state) ? state : ButtonState.None;
    }

    public bool JustPressed(Key key)
    {
        return GetKeyState(key) is ButtonState.JustPressed;
    }

    public bool Pressed(Key key)
    {
        return GetKeyState(key) is ButtonState.Pressed or ButtonState.JustPressed;
    }

    public bool JustReleased(Key key)
    {
        return GetKeyState(key) is ButtonState.JustReleased;
    }

    public float GetAxis(Key negative, Key positive)
    {
        float value = 0;
        if (Pressed(positive)) value += 1;
        if (Pressed(negative)) value -= 1;
        return value;
    }

    public Vec2f GetAxis2D(Key left, Key right, Key up, Key down)
    {
        float x = 0;
        float y = 0;
        if (Pressed(right)) x += 1;
        if (Pressed(left)) x -= 1;
        if (Pressed(up)) y += 1;
        if (Pressed(down)) y -= 1;

        return new Vec2f(x, y);
    }

    public void Dispose()
    {

    }
}
