namespace Pollus.Engine.Input;

static class SDLMapping
{
    public static Key MapKey(Silk.NET.SDL.Scancode scancode)
    {
        return scancode switch
        {
            Silk.NET.SDL.Scancode.ScancodeA => Key.KeyA,
            Silk.NET.SDL.Scancode.ScancodeB => Key.KeyB,
            Silk.NET.SDL.Scancode.ScancodeC => Key.KeyC,
            Silk.NET.SDL.Scancode.ScancodeD => Key.KeyD,
            Silk.NET.SDL.Scancode.ScancodeE => Key.KeyE,
            Silk.NET.SDL.Scancode.ScancodeF => Key.KeyF,
            Silk.NET.SDL.Scancode.ScancodeG => Key.KeyG,
            Silk.NET.SDL.Scancode.ScancodeH => Key.KeyH,
            Silk.NET.SDL.Scancode.ScancodeI => Key.KeyI,
            Silk.NET.SDL.Scancode.ScancodeJ => Key.KeyJ,
            Silk.NET.SDL.Scancode.ScancodeK => Key.KeyK,
            Silk.NET.SDL.Scancode.ScancodeL => Key.KeyL,
            Silk.NET.SDL.Scancode.ScancodeM => Key.KeyM,
            Silk.NET.SDL.Scancode.ScancodeN => Key.KeyN,
            Silk.NET.SDL.Scancode.ScancodeO => Key.KeyO,
            Silk.NET.SDL.Scancode.ScancodeP => Key.KeyP,
            Silk.NET.SDL.Scancode.ScancodeQ => Key.KeyQ,
            Silk.NET.SDL.Scancode.ScancodeR => Key.KeyR,
            Silk.NET.SDL.Scancode.ScancodeS => Key.KeyS,
            Silk.NET.SDL.Scancode.ScancodeT => Key.KeyT,
            Silk.NET.SDL.Scancode.ScancodeU => Key.KeyU,
            Silk.NET.SDL.Scancode.ScancodeV => Key.KeyV,
            Silk.NET.SDL.Scancode.ScancodeW => Key.KeyW,
            Silk.NET.SDL.Scancode.ScancodeX => Key.KeyX,
            Silk.NET.SDL.Scancode.ScancodeY => Key.KeyY,
            Silk.NET.SDL.Scancode.ScancodeZ => Key.KeyZ,
            Silk.NET.SDL.Scancode.Scancode1 => Key.Digit1,
            Silk.NET.SDL.Scancode.Scancode2 => Key.Digit2,
            Silk.NET.SDL.Scancode.Scancode3 => Key.Digit3,
            Silk.NET.SDL.Scancode.Scancode4 => Key.Digit4,
            Silk.NET.SDL.Scancode.Scancode5 => Key.Digit5,
            Silk.NET.SDL.Scancode.Scancode6 => Key.Digit6,
            Silk.NET.SDL.Scancode.Scancode7 => Key.Digit7,
            Silk.NET.SDL.Scancode.Scancode8 => Key.Digit8,
            Silk.NET.SDL.Scancode.Scancode9 => Key.Digit9,
            Silk.NET.SDL.Scancode.Scancode0 => Key.Digit0,
            Silk.NET.SDL.Scancode.ScancodeReturn => Key.Enter,
            Silk.NET.SDL.Scancode.ScancodeEscape => Key.Escape,
            Silk.NET.SDL.Scancode.ScancodeBackspace => Key.Backspace,
            Silk.NET.SDL.Scancode.ScancodeTab => Key.Tab,
            Silk.NET.SDL.Scancode.ScancodeSpace => Key.Space,
            Silk.NET.SDL.Scancode.ScancodeMinus => Key.Minus,
            Silk.NET.SDL.Scancode.ScancodeEquals => Key.Equal,
            Silk.NET.SDL.Scancode.ScancodeLeftbracket => Key.LeftBracket,
            Silk.NET.SDL.Scancode.ScancodeRightbracket => Key.RightBracket,
            Silk.NET.SDL.Scancode.ScancodeBackslash => Key.Backslash,
            Silk.NET.SDL.Scancode.ScancodeNonushash => Key.NonUSHash,
            Silk.NET.SDL.Scancode.ScancodeSemicolon => Key.Semicolon,
            Silk.NET.SDL.Scancode.ScancodeApostrophe => Key.Apostrophe,
            Silk.NET.SDL.Scancode.ScancodeGrave => Key.Grave,
            Silk.NET.SDL.Scancode.ScancodeComma => Key.Comma,
            Silk.NET.SDL.Scancode.ScancodePeriod => Key.Period,
            Silk.NET.SDL.Scancode.ScancodeSlash => Key.Slash,
            Silk.NET.SDL.Scancode.ScancodeCapslock => Key.CapsLock,
            Silk.NET.SDL.Scancode.ScancodeF1 => Key.F1,
            Silk.NET.SDL.Scancode.ScancodeF2 => Key.F2,
            Silk.NET.SDL.Scancode.ScancodeF3 => Key.F3,
            Silk.NET.SDL.Scancode.ScancodeF4 => Key.F4,
            Silk.NET.SDL.Scancode.ScancodeF5 => Key.F5,
            Silk.NET.SDL.Scancode.ScancodeF6 => Key.F6,
            Silk.NET.SDL.Scancode.ScancodeF7 => Key.F7,
            Silk.NET.SDL.Scancode.ScancodeF8 => Key.F8,
            Silk.NET.SDL.Scancode.ScancodeF9 => Key.F9,
            Silk.NET.SDL.Scancode.ScancodeF10 => Key.F10,
            Silk.NET.SDL.Scancode.ScancodeF11 => Key.F11,
            Silk.NET.SDL.Scancode.ScancodeF12 => Key.F12,
            Silk.NET.SDL.Scancode.ScancodePrintscreen => Key.PrintScreen,
            Silk.NET.SDL.Scancode.ScancodeScrolllock => Key.ScrollLock,
            Silk.NET.SDL.Scancode.ScancodePause => Key.Pause,
            Silk.NET.SDL.Scancode.ScancodeInsert => Key.Insert,
            Silk.NET.SDL.Scancode.ScancodeHome => Key.Home,
            Silk.NET.SDL.Scancode.ScancodePageup => Key.PageUp,
            Silk.NET.SDL.Scancode.ScancodeDelete => Key.Delete,
            Silk.NET.SDL.Scancode.ScancodeEnd => Key.End,
            Silk.NET.SDL.Scancode.ScancodePagedown => Key.PageDown,
            Silk.NET.SDL.Scancode.ScancodeRight => Key.ArrowRight,
            Silk.NET.SDL.Scancode.ScancodeLeft => Key.ArrowLeft,
            Silk.NET.SDL.Scancode.ScancodeDown => Key.ArrowDown,
            Silk.NET.SDL.Scancode.ScancodeUp => Key.ArrowUp,
            Silk.NET.SDL.Scancode.ScancodeLctrl => Key.LeftControl,
            Silk.NET.SDL.Scancode.ScancodeLshift => Key.LeftShift,
            Silk.NET.SDL.Scancode.ScancodeLalt => Key.LeftAlt,
            Silk.NET.SDL.Scancode.ScancodeLgui => Key.LeftMeta,
            Silk.NET.SDL.Scancode.ScancodeRctrl => Key.RightControl,
            Silk.NET.SDL.Scancode.ScancodeRshift => Key.RightShift,
            Silk.NET.SDL.Scancode.ScancodeRalt => Key.RightAlt,
            Silk.NET.SDL.Scancode.ScancodeRgui => Key.RightMeta,
            _ => Key.Unknown,
        };
    }

    public static MouseButton MapMouseButton(byte button)
    {
        return button switch
        {
            1 => MouseButton.Left,
            2 => MouseButton.Middle,
            3 => MouseButton.Right,
            4 => MouseButton.Button4,
            5 => MouseButton.Button5,
            6 => MouseButton.Button6,
            7 => MouseButton.Button7,
            8 => MouseButton.Button8,
            _ => MouseButton.Unknown,
        };
    }

    public static GamepadAxis MapGamepadAxis(Silk.NET.SDL.GameControllerAxis axis)
    {
        return axis switch
        {
            Silk.NET.SDL.GameControllerAxis.Leftx => GamepadAxis.LeftX,
            Silk.NET.SDL.GameControllerAxis.Lefty => GamepadAxis.LeftY,
            Silk.NET.SDL.GameControllerAxis.Rightx => GamepadAxis.RightX,
            Silk.NET.SDL.GameControllerAxis.Righty => GamepadAxis.RightY,
            Silk.NET.SDL.GameControllerAxis.Triggerleft => GamepadAxis.LeftZ,
            Silk.NET.SDL.GameControllerAxis.Triggerright => GamepadAxis.RightZ,
            _ => GamepadAxis.Unknown,
        };
    }

    public static GamepadButton MapGamepadButton(Silk.NET.SDL.GameControllerButton button)
    {
        return button switch
        {
            Silk.NET.SDL.GameControllerButton.A => GamepadButton.South,
            Silk.NET.SDL.GameControllerButton.B => GamepadButton.East,
            Silk.NET.SDL.GameControllerButton.X => GamepadButton.West,
            Silk.NET.SDL.GameControllerButton.Y => GamepadButton.North,
            Silk.NET.SDL.GameControllerButton.Back => GamepadButton.Back,
            Silk.NET.SDL.GameControllerButton.Guide => GamepadButton.Guide,
            Silk.NET.SDL.GameControllerButton.Start => GamepadButton.Start,
            Silk.NET.SDL.GameControllerButton.Leftstick => GamepadButton.LeftStick,
            Silk.NET.SDL.GameControllerButton.Rightstick => GamepadButton.RightStick,
            Silk.NET.SDL.GameControllerButton.Leftshoulder => GamepadButton.LeftShoulder,
            Silk.NET.SDL.GameControllerButton.Rightshoulder => GamepadButton.RightShoulder,
            Silk.NET.SDL.GameControllerButton.DpadUp => GamepadButton.DPadUp,
            Silk.NET.SDL.GameControllerButton.DpadDown => GamepadButton.DPadDown,
            Silk.NET.SDL.GameControllerButton.DpadLeft => GamepadButton.DPadLeft,
            Silk.NET.SDL.GameControllerButton.DpadRight => GamepadButton.DPadRight,
            _ => GamepadButton.Unknown,
        };
    }
}