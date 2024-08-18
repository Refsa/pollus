using System.ComponentModel.Design;
using Pollus.Mathematics;

namespace Pollus.Engine.Input;

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
    Digit0,
    Digit1,
    Digit2,
    Digit3,
    Digit4,
    Digit5,
    Digit6,
    Digit7,
    Digit8,
    Digit9,
    KeyA,
    KeyB,
    KeyC,
    KeyD,
    KeyE,
    KeyF,
    KeyG,
    KeyH,
    KeyI,
    KeyJ,
    KeyK,
    KeyL,
    KeyM,
    KeyN,
    KeyO,
    KeyP,
    KeyQ,
    KeyR,
    KeyS,
    KeyT,
    KeyU,
    KeyV,
    KeyW,
    KeyX,
    KeyY,
    KeyZ,
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
}

public class Keyboard : IInputDevice, IButtonInputDevice<Key>
{
    public nint ExternalId { get; }
    public Guid Id { get; } = new();
    public int Index { get; set; }
    public InputType Type => InputType.Keyboard;

    Dictionary<Key, ButtonState> buttonStates = new();
    HashSet<Key> changed = new();

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

        buttonStates[key] = state;
        changed.Add(key);
    }

    public void Update()
    {
        foreach (var key in buttonStates.Keys)
        {
            if (!changed.Contains(key))
            {
                buttonStates[key] = buttonStates[key] switch
                {
                    ButtonState.JustPressed => ButtonState.Pressed,
                    ButtonState.JustReleased => ButtonState.None,
                    _ => buttonStates[key]
                };
            }
        }
        changed.Clear();
    }

    public ButtonState GetKeyState(Key key)
    {
        return buttonStates.TryGetValue(key, out var state) ? state : ButtonState.None;
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