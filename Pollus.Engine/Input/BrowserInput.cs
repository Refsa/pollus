namespace Pollus.Engine.Input;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Utils;

public class BrowserInput : Input
{
    Guid keyboardId;

    public BrowserInput()
    {
        var sdlFlags = (uint)(SDL_InitFlag.InitEvents | SDL_InitFlag.InitGamecontroller | SDL_InitFlag.InitJoystick);
        if (EmscriptenSDL.WasInit(sdlFlags) is false)
        {
            if (EmscriptenSDL.AnyInitialized())
            {
                EmscriptenSDL.InitSubSystem(sdlFlags);
            }
            else
            {
                EmscriptenSDL.Init(sdlFlags);
            }
        }

        // Currently on supports a single keyboard
        {
            var keyboard = new Keyboard();
            keyboardId = keyboard.Id;
            AddDevice(keyboard);
        }
    }

    protected override void Dispose(bool disposing)
    {
        EmscriptenSDL.Quit();
    }

    unsafe protected override void UpdateInternal()
    {
        var keyboard = GetDevice(keyboardId) as Keyboard;

        EmscriptenSDL.PumpEvents();
        var @event = new SDL_Event();
        while (EmscriptenSDL.PollEvent(ref @event) == 1)
        {
            switch (@event.Type)
            {
                case SDL_EventType.QUIT:
                    break;
                case SDL_EventType.KEYDOWN:
                case SDL_EventType.KEYUP:
                    SDL_KeyboardEvent keyEvent = Unsafe.Read<SDL_KeyboardEvent>(Unsafe.AsPointer(ref @event));
                    var key = MapKey(keyEvent.Keysym.Scancode);
                    if (keyEvent.Repeat == 0)
                    {
                        keyboard?.SetKeyState(key, keyEvent.State == 1);
                    }
                    break;
            }
        }
    }

    ModifierKey MapModifier(SDL_Keymod mod)
    {
        return mod switch
        {
            SDL_Keymod.LSHIFT => ModifierKey.LeftShift,
            SDL_Keymod.RSHIFT => ModifierKey.RightShift,
            SDL_Keymod.LCTRL => ModifierKey.LeftControl,
            SDL_Keymod.RCTRL => ModifierKey.RightControl,
            SDL_Keymod.LALT => ModifierKey.LeftAlt,
            SDL_Keymod.RALT => ModifierKey.RightAlt,
            SDL_Keymod.LGUI => ModifierKey.LeftMeta,
            SDL_Keymod.RGUI => ModifierKey.RightMeta,
            SDL_Keymod.NUM => ModifierKey.NumLock,
            SDL_Keymod.CAPS => ModifierKey.CapsLock,
            SDL_Keymod.MODE => ModifierKey.Mode,
            _ => ModifierKey.None,
        };
    }

    Key MapKey(SDL_Scancode scancode)
    {
        return scancode switch
        {
            SDL_Scancode.SDL_SCANCODE_A => Key.KeyA,
            SDL_Scancode.SDL_SCANCODE_B => Key.KeyB,
            SDL_Scancode.SDL_SCANCODE_C => Key.KeyC,
            SDL_Scancode.SDL_SCANCODE_D => Key.KeyD,
            SDL_Scancode.SDL_SCANCODE_E => Key.KeyE,
            SDL_Scancode.SDL_SCANCODE_F => Key.KeyF,
            SDL_Scancode.SDL_SCANCODE_G => Key.KeyG,
            SDL_Scancode.SDL_SCANCODE_H => Key.KeyH,
            SDL_Scancode.SDL_SCANCODE_I => Key.KeyI,
            SDL_Scancode.SDL_SCANCODE_J => Key.KeyJ,
            SDL_Scancode.SDL_SCANCODE_K => Key.KeyK,
            SDL_Scancode.SDL_SCANCODE_L => Key.KeyL,
            SDL_Scancode.SDL_SCANCODE_M => Key.KeyM,
            SDL_Scancode.SDL_SCANCODE_N => Key.KeyN,
            SDL_Scancode.SDL_SCANCODE_O => Key.KeyO,
            SDL_Scancode.SDL_SCANCODE_P => Key.KeyP,
            SDL_Scancode.SDL_SCANCODE_Q => Key.KeyQ,
            SDL_Scancode.SDL_SCANCODE_R => Key.KeyR,
            SDL_Scancode.SDL_SCANCODE_S => Key.KeyS,
            SDL_Scancode.SDL_SCANCODE_T => Key.KeyT,
            SDL_Scancode.SDL_SCANCODE_U => Key.KeyU,
            SDL_Scancode.SDL_SCANCODE_V => Key.KeyV,
            SDL_Scancode.SDL_SCANCODE_W => Key.KeyW,
            SDL_Scancode.SDL_SCANCODE_X => Key.KeyX,
            SDL_Scancode.SDL_SCANCODE_Y => Key.KeyY,
            SDL_Scancode.SDL_SCANCODE_Z => Key.KeyZ,
            SDL_Scancode.SDL_SCANCODE_1 => Key.Digit1,
            SDL_Scancode.SDL_SCANCODE_2 => Key.Digit2,
            SDL_Scancode.SDL_SCANCODE_3 => Key.Digit3,
            SDL_Scancode.SDL_SCANCODE_4 => Key.Digit4,
            SDL_Scancode.SDL_SCANCODE_5 => Key.Digit5,
            SDL_Scancode.SDL_SCANCODE_6 => Key.Digit6,
            SDL_Scancode.SDL_SCANCODE_7 => Key.Digit7,
            SDL_Scancode.SDL_SCANCODE_8 => Key.Digit8,
            SDL_Scancode.SDL_SCANCODE_9 => Key.Digit9,
            SDL_Scancode.SDL_SCANCODE_0 => Key.Digit0,
            SDL_Scancode.SDL_SCANCODE_RETURN => Key.Enter,
            SDL_Scancode.SDL_SCANCODE_ESCAPE => Key.Escape,
            SDL_Scancode.SDL_SCANCODE_BACKSPACE => Key.Backspace,
            SDL_Scancode.SDL_SCANCODE_TAB => Key.Tab,
            SDL_Scancode.SDL_SCANCODE_SPACE => Key.Space,
            SDL_Scancode.SDL_SCANCODE_MINUS => Key.Minus,
            SDL_Scancode.SDL_SCANCODE_EQUALS => Key.Equal,
            SDL_Scancode.SDL_SCANCODE_LEFTBRACKET => Key.LeftBracket,
            SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET => Key.RightBracket,
            SDL_Scancode.SDL_SCANCODE_BACKSLASH => Key.Backslash,
            SDL_Scancode.SDL_SCANCODE_NONUSHASH => Key.NonUSHash,
            SDL_Scancode.SDL_SCANCODE_SEMICOLON => Key.Semicolon,
            SDL_Scancode.SDL_SCANCODE_APOSTROPHE => Key.Apostrophe,
            SDL_Scancode.SDL_SCANCODE_GRAVE => Key.Grave,
            SDL_Scancode.SDL_SCANCODE_COMMA => Key.Comma,
            SDL_Scancode.SDL_SCANCODE_PERIOD => Key.Period,
            SDL_Scancode.SDL_SCANCODE_SLASH => Key.Slash,
            SDL_Scancode.SDL_SCANCODE_CAPSLOCK => Key.CapsLock,
            SDL_Scancode.SDL_SCANCODE_F1 => Key.F1,
            SDL_Scancode.SDL_SCANCODE_F2 => Key.F2,
            SDL_Scancode.SDL_SCANCODE_F3 => Key.F3,
            SDL_Scancode.SDL_SCANCODE_F4 => Key.F4,
            SDL_Scancode.SDL_SCANCODE_F5 => Key.F5,
            SDL_Scancode.SDL_SCANCODE_F6 => Key.F6,
            SDL_Scancode.SDL_SCANCODE_F7 => Key.F7,
            SDL_Scancode.SDL_SCANCODE_F8 => Key.F8,
            SDL_Scancode.SDL_SCANCODE_F9 => Key.F9,
            SDL_Scancode.SDL_SCANCODE_F10 => Key.F10,
            SDL_Scancode.SDL_SCANCODE_F11 => Key.F11,
            SDL_Scancode.SDL_SCANCODE_F12 => Key.F12,
            SDL_Scancode.SDL_SCANCODE_PRINTSCREEN => Key.PrintScreen,
            SDL_Scancode.SDL_SCANCODE_SCROLLLOCK => Key.ScrollLock,
            SDL_Scancode.SDL_SCANCODE_PAUSE => Key.Pause,
            SDL_Scancode.SDL_SCANCODE_INSERT => Key.Insert,
            SDL_Scancode.SDL_SCANCODE_HOME => Key.Home,
            SDL_Scancode.SDL_SCANCODE_PAGEUP => Key.PageUp,
            SDL_Scancode.SDL_SCANCODE_DELETE => Key.Delete,
            SDL_Scancode.SDL_SCANCODE_END => Key.End,
            SDL_Scancode.SDL_SCANCODE_PAGEDOWN => Key.PageDown,
            SDL_Scancode.SDL_SCANCODE_RIGHT => Key.ArrowRight,
            SDL_Scancode.SDL_SCANCODE_LEFT => Key.ArrowLeft,
            SDL_Scancode.SDL_SCANCODE_DOWN => Key.ArrowDown,
            SDL_Scancode.SDL_SCANCODE_UP => Key.ArrowUp,
            _ => Key.Unknown,
        };
    }
}

class EmscriptenSDL
{
    [DllImport("__Internal_emscripten")]
    extern static int SDL_Init(uint flags);

    [DllImport("__Internal_emscripten")]
    extern static int SDL_InitSubSystem(uint flags);

    [DllImport("__Internal_emscripten")]
    extern static uint SDL_WasInit(uint flags);

    [DllImport("__Internal_emscripten")]
    extern static void SDL_Quit();

    [DllImport("__Internal_emscripten")]
    extern static void SDL_QuitSubSystem(uint flags);

    [DllImport("__Internal_emscripten")]
    extern static void SDL_PumpEvents();

    [DllImport("__Internal_emscripten")]
    unsafe extern static int SDL_PollEvent(SDL_Event* @event);

    public static void Init(uint flags)
    {
        SDL_Init(flags);
    }

    public static void InitSubSystem(uint flags)
    {
        SDL_InitSubSystem(flags);
    }

    public static bool WasInit(uint flags)
    {
        var initializedFlags = SDL_WasInit(flags);
        return (initializedFlags & flags) == flags;
    }

    public static bool AnyInitialized()
    {
        return SDL_WasInit(~0u) != 0;
    }

    public static void Quit()
    {
        SDL_Quit();
    }

    public static void QuitSubSystem(uint flags)
    {
        SDL_QuitSubSystem(flags);
    }

    public static void PumpEvents()
    {
        SDL_PumpEvents();
    }

    unsafe public static int PollEvent(ref SDL_Event @event)
    {
        fixed (SDL_Event* ptr = &@event)
        {
            return SDL_PollEvent(ptr);
        }
    }
}

[Flags]
enum SDL_InitFlag : uint
{
    InitVideo = 32u,
    InitJoystick = 512u,
    InitEvents = 16384u,
    InitGamecontroller = 8192u,
}

struct SDL_Event
{
    public SDL_EventType Type;
    Padding padding;

    [InlineArray(64)]
    struct Padding
    {
        byte _firstElement;
    }
}

struct SDL_KeyboardEvent
{
    public SDL_EventType Type; /**< ::SDL_KEYDOWN or ::SDL_KEYUP */
    public uint WindowID;      /**< The window with keyboard focus, if any */
    public byte State;         /**< ::SDL_PRESSED or ::SDL_RELEASED */
    public byte Repeat;        /**< Non-zero if this is a key repeat */
    byte padding2;
    byte padding3;
    public SDL_Keysym Keysym;  /**< The key that was pressed or released */
}

struct SDL_Keysym
{
    public SDL_Scancode Scancode;      /**< SDL physical key code - see ::SDL_Scancode for details */
    int sym;            /**< SDL virtual key code - see ::SDL_Keycode for details */
    public ushort Mod;                 /**< current key modifiers */
    uint unicode;             /**< \deprecated use SDL_TextInputEvent instead */
}

enum SDL_Scancode : int
{
    SDL_SCANCODE_UNKNOWN = 0,

    SDL_SCANCODE_A = 4,
    SDL_SCANCODE_B = 5,
    SDL_SCANCODE_C = 6,
    SDL_SCANCODE_D = 7,
    SDL_SCANCODE_E = 8,
    SDL_SCANCODE_F = 9,
    SDL_SCANCODE_G = 10,
    SDL_SCANCODE_H = 11,
    SDL_SCANCODE_I = 12,
    SDL_SCANCODE_J = 13,
    SDL_SCANCODE_K = 14,
    SDL_SCANCODE_L = 15,
    SDL_SCANCODE_M = 16,
    SDL_SCANCODE_N = 17,
    SDL_SCANCODE_O = 18,
    SDL_SCANCODE_P = 19,
    SDL_SCANCODE_Q = 20,
    SDL_SCANCODE_R = 21,
    SDL_SCANCODE_S = 22,
    SDL_SCANCODE_T = 23,
    SDL_SCANCODE_U = 24,
    SDL_SCANCODE_V = 25,
    SDL_SCANCODE_W = 26,
    SDL_SCANCODE_X = 27,
    SDL_SCANCODE_Y = 28,
    SDL_SCANCODE_Z = 29,

    SDL_SCANCODE_1 = 30,
    SDL_SCANCODE_2 = 31,
    SDL_SCANCODE_3 = 32,
    SDL_SCANCODE_4 = 33,
    SDL_SCANCODE_5 = 34,
    SDL_SCANCODE_6 = 35,
    SDL_SCANCODE_7 = 36,
    SDL_SCANCODE_8 = 37,
    SDL_SCANCODE_9 = 38,
    SDL_SCANCODE_0 = 39,

    SDL_SCANCODE_RETURN = 40,
    SDL_SCANCODE_ESCAPE = 41,
    SDL_SCANCODE_BACKSPACE = 42,
    SDL_SCANCODE_TAB = 43,
    SDL_SCANCODE_SPACE = 44,

    SDL_SCANCODE_MINUS = 45,
    SDL_SCANCODE_EQUALS = 46,
    SDL_SCANCODE_LEFTBRACKET = 47,
    SDL_SCANCODE_RIGHTBRACKET = 48,
    SDL_SCANCODE_BACKSLASH = 49,
    SDL_SCANCODE_NONUSHASH = 50,
    SDL_SCANCODE_SEMICOLON = 51,
    SDL_SCANCODE_APOSTROPHE = 52,
    SDL_SCANCODE_GRAVE = 53,
    SDL_SCANCODE_COMMA = 54,
    SDL_SCANCODE_PERIOD = 55,
    SDL_SCANCODE_SLASH = 56,

    SDL_SCANCODE_CAPSLOCK = 57,

    SDL_SCANCODE_F1 = 58,
    SDL_SCANCODE_F2 = 59,
    SDL_SCANCODE_F3 = 60,
    SDL_SCANCODE_F4 = 61,
    SDL_SCANCODE_F5 = 62,
    SDL_SCANCODE_F6 = 63,
    SDL_SCANCODE_F7 = 64,
    SDL_SCANCODE_F8 = 65,
    SDL_SCANCODE_F9 = 66,
    SDL_SCANCODE_F10 = 67,
    SDL_SCANCODE_F11 = 68,
    SDL_SCANCODE_F12 = 69,

    SDL_SCANCODE_PRINTSCREEN = 70,
    SDL_SCANCODE_SCROLLLOCK = 71,
    SDL_SCANCODE_PAUSE = 72,
    SDL_SCANCODE_INSERT = 73, /**< insert on PC, help on some Mac keyboards (but
                                   does send code 73, not 117) */
    SDL_SCANCODE_HOME = 74,
    SDL_SCANCODE_PAGEUP = 75,
    SDL_SCANCODE_DELETE = 76,
    SDL_SCANCODE_END = 77,
    SDL_SCANCODE_PAGEDOWN = 78,
    SDL_SCANCODE_RIGHT = 79,
    SDL_SCANCODE_LEFT = 80,
    SDL_SCANCODE_DOWN = 81,
    SDL_SCANCODE_UP = 82,

    SDL_SCANCODE_NUMLOCKCLEAR = 83, /**< num lock on PC, clear on Mac keyboards 
                                     */
    SDL_SCANCODE_KP_DIVIDE = 84,
    SDL_SCANCODE_KP_MULTIPLY = 85,
    SDL_SCANCODE_KP_MINUS = 86,
    SDL_SCANCODE_KP_PLUS = 87,
    SDL_SCANCODE_KP_ENTER = 88,
    SDL_SCANCODE_KP_1 = 89,
    SDL_SCANCODE_KP_2 = 90,
    SDL_SCANCODE_KP_3 = 91,
    SDL_SCANCODE_KP_4 = 92,
    SDL_SCANCODE_KP_5 = 93,
    SDL_SCANCODE_KP_6 = 94,
    SDL_SCANCODE_KP_7 = 95,
    SDL_SCANCODE_KP_8 = 96,
    SDL_SCANCODE_KP_9 = 97,
    SDL_SCANCODE_KP_0 = 98,
    SDL_SCANCODE_KP_PERIOD = 99,
    SDL_SCANCODE_NONUSBACKSLASH = 100,
    SDL_SCANCODE_APPLICATION = 101,
    SDL_SCANCODE_POWER = 102,
    SDL_SCANCODE_KP_EQUALS = 103,
    SDL_SCANCODE_F13 = 104,
    SDL_SCANCODE_F14 = 105,
    SDL_SCANCODE_F15 = 106,
    SDL_SCANCODE_F16 = 107,
    SDL_SCANCODE_F17 = 108,
    SDL_SCANCODE_F18 = 109,
    SDL_SCANCODE_F19 = 110,
    SDL_SCANCODE_F20 = 111,
    SDL_SCANCODE_F21 = 112,
    SDL_SCANCODE_F22 = 113,
    SDL_SCANCODE_F23 = 114,
    SDL_SCANCODE_F24 = 115,
    SDL_SCANCODE_EXECUTE = 116,
    SDL_SCANCODE_HELP = 117,
    SDL_SCANCODE_MENU = 118,
    SDL_SCANCODE_SELECT = 119,
    SDL_SCANCODE_STOP = 120,
    SDL_SCANCODE_AGAIN = 121,
    SDL_SCANCODE_UNDO = 122,
    SDL_SCANCODE_CUT = 123,
    SDL_SCANCODE_COPY = 124,
    SDL_SCANCODE_PASTE = 125,
    SDL_SCANCODE_FIND = 126,
    SDL_SCANCODE_MUTE = 127,
    SDL_SCANCODE_VOLUMEUP = 128,
    SDL_SCANCODE_VOLUMEDOWN = 129,
    /* not sure whether there's a reason to enable these */
    /*     SDL_SCANCODE_LOCKINGCAPSLOCK = 130,  */
    /*     SDL_SCANCODE_LOCKINGNUMLOCK = 131, */
    /*     SDL_SCANCODE_LOCKINGSCROLLLOCK = 132, */
    SDL_SCANCODE_KP_COMMA = 133,
    SDL_SCANCODE_KP_EQUALSAS400 = 134,
    SDL_SCANCODE_INTERNATIONAL1 = 135,
    SDL_SCANCODE_INTERNATIONAL2 = 136,
    SDL_SCANCODE_INTERNATIONAL3 = 137,
    SDL_SCANCODE_INTERNATIONAL4 = 138,
    SDL_SCANCODE_INTERNATIONAL5 = 139,
    SDL_SCANCODE_INTERNATIONAL6 = 140,
    SDL_SCANCODE_INTERNATIONAL7 = 141,
    SDL_SCANCODE_INTERNATIONAL8 = 142,
    SDL_SCANCODE_INTERNATIONAL9 = 143,
    SDL_SCANCODE_LANG1 = 144, /**< Hangul/English toggle */
    SDL_SCANCODE_LANG2 = 145, /**< Hanja conversion */
    SDL_SCANCODE_LANG3 = 146, /**< Katakana */
    SDL_SCANCODE_LANG4 = 147, /**< Hiragana */
    SDL_SCANCODE_LANG5 = 148, /**< Zenkaku/Hankaku */
    SDL_SCANCODE_LANG6 = 149, /**< reserved */
    SDL_SCANCODE_LANG7 = 150, /**< reserved */
    SDL_SCANCODE_LANG8 = 151, /**< reserved */
    SDL_SCANCODE_LANG9 = 152, /**< reserved */

    SDL_SCANCODE_ALTERASE = 153,
    SDL_SCANCODE_SYSREQ = 154,
    SDL_SCANCODE_CANCEL = 155,
    SDL_SCANCODE_CLEAR = 156,
    SDL_SCANCODE_PRIOR = 157,
    SDL_SCANCODE_RETURN2 = 158,
    SDL_SCANCODE_SEPARATOR = 159,
    SDL_SCANCODE_OUT = 160,
    SDL_SCANCODE_OPER = 161,
    SDL_SCANCODE_CLEARAGAIN = 162,
    SDL_SCANCODE_CRSEL = 163,
    SDL_SCANCODE_EXSEL = 164,

    SDL_SCANCODE_KP_00 = 176,
    SDL_SCANCODE_KP_000 = 177,
    SDL_SCANCODE_THOUSANDSSEPARATOR = 178,
    SDL_SCANCODE_DECIMALSEPARATOR = 179,
    SDL_SCANCODE_CURRENCYUNIT = 180,
    SDL_SCANCODE_CURRENCYSUBUNIT = 181,
    SDL_SCANCODE_KP_LEFTPAREN = 182,
    SDL_SCANCODE_KP_RIGHTPAREN = 183,
    SDL_SCANCODE_KP_LEFTBRACE = 184,
    SDL_SCANCODE_KP_RIGHTBRACE = 185,
    SDL_SCANCODE_KP_TAB = 186,
    SDL_SCANCODE_KP_BACKSPACE = 187,
    SDL_SCANCODE_KP_A = 188,
    SDL_SCANCODE_KP_B = 189,
    SDL_SCANCODE_KP_C = 190,
    SDL_SCANCODE_KP_D = 191,
    SDL_SCANCODE_KP_E = 192,
    SDL_SCANCODE_KP_F = 193,
    SDL_SCANCODE_KP_XOR = 194,
    SDL_SCANCODE_KP_POWER = 195,
    SDL_SCANCODE_KP_PERCENT = 196,
    SDL_SCANCODE_KP_LESS = 197,
    SDL_SCANCODE_KP_GREATER = 198,
    SDL_SCANCODE_KP_AMPERSAND = 199,
    SDL_SCANCODE_KP_DBLAMPERSAND = 200,
    SDL_SCANCODE_KP_VERTICALBAR = 201,
    SDL_SCANCODE_KP_DBLVERTICALBAR = 202,
    SDL_SCANCODE_KP_COLON = 203,
    SDL_SCANCODE_KP_HASH = 204,
    SDL_SCANCODE_KP_SPACE = 205,
    SDL_SCANCODE_KP_AT = 206,
    SDL_SCANCODE_KP_EXCLAM = 207,
    SDL_SCANCODE_KP_MEMSTORE = 208,
    SDL_SCANCODE_KP_MEMRECALL = 209,
    SDL_SCANCODE_KP_MEMCLEAR = 210,
    SDL_SCANCODE_KP_MEMADD = 211,
    SDL_SCANCODE_KP_MEMSUBTRACT = 212,
    SDL_SCANCODE_KP_MEMMULTIPLY = 213,
    SDL_SCANCODE_KP_MEMDIVIDE = 214,
    SDL_SCANCODE_KP_PLUSMINUS = 215,
    SDL_SCANCODE_KP_CLEAR = 216,
    SDL_SCANCODE_KP_CLEARENTRY = 217,
    SDL_SCANCODE_KP_BINARY = 218,
    SDL_SCANCODE_KP_OCTAL = 219,
    SDL_SCANCODE_KP_DECIMAL = 220,
    SDL_SCANCODE_KP_HEXADECIMAL = 221,

    SDL_SCANCODE_LCTRL = 224,
    SDL_SCANCODE_LSHIFT = 225,
    SDL_SCANCODE_LALT = 226, /**< alt, option */
    SDL_SCANCODE_LGUI = 227, /**< windows, command (apple), meta */
    SDL_SCANCODE_RCTRL = 228,
    SDL_SCANCODE_RSHIFT = 229,
    SDL_SCANCODE_RALT = 230, /**< alt gr, option */
    SDL_SCANCODE_RGUI = 231, /**< windows, command (apple), meta */

    SDL_SCANCODE_MODE = 257,

    SDL_SCANCODE_AUDIONEXT = 258,
    SDL_SCANCODE_AUDIOPREV = 259,
    SDL_SCANCODE_AUDIOSTOP = 260,
    SDL_SCANCODE_AUDIOPLAY = 261,
    SDL_SCANCODE_AUDIOMUTE = 262,
    SDL_SCANCODE_MEDIASELECT = 263,
    SDL_SCANCODE_WWW = 264,
    SDL_SCANCODE_MAIL = 265,
    SDL_SCANCODE_CALCULATOR = 266,
    SDL_SCANCODE_COMPUTER = 267,
    SDL_SCANCODE_AC_SEARCH = 268,
    SDL_SCANCODE_AC_HOME = 269,
    SDL_SCANCODE_AC_BACK = 270,
    SDL_SCANCODE_AC_FORWARD = 271,
    SDL_SCANCODE_AC_STOP = 272,
    SDL_SCANCODE_AC_REFRESH = 273,
    SDL_SCANCODE_AC_BOOKMARKS = 274,

    SDL_SCANCODE_BRIGHTNESSDOWN = 275,
    SDL_SCANCODE_BRIGHTNESSUP = 276,
    SDL_SCANCODE_DISPLAYSWITCH = 277,
    SDL_SCANCODE_KBDILLUMTOGGLE = 278,
    SDL_SCANCODE_KBDILLUMDOWN = 279,
    SDL_SCANCODE_KBDILLUMUP = 280,
    SDL_SCANCODE_EJECT = 281,
    SDL_SCANCODE_SLEEP = 282,

    SDL_NUM_SCANCODES = 512
}

[Flags]
public enum SDL_Keymod : ushort
{
    NONE = 0x0000,
    LSHIFT = 0x0001,
    RSHIFT = 0x0002,
    LCTRL = 0x0040,
    RCTRL = 0x0080,
    LALT = 0x0100,
    RALT = 0x0200,
    LGUI = 0x0400,
    RGUI = 0x0800,
    NUM = 0x1000,
    CAPS = 0x2000,
    MODE = 0x4000,
    RESERVED = 0x8000
}

enum SDL_EventType : uint
{
    SDL_NOEVENT = 0,
    SDL_FIRSTEVENT = 0,     /**< Unused (do not remove) */

    /* Application events */
    QUIT = 0x100, /**< User-requested quit */

    /* Window events */
    SDL_WINDOWEVENT = 0x200, /**< Window state change */
    SDL_SYSWMEVENT,             /**< System specific event */

    /* Keyboard events */
    KEYDOWN = 0x300, /**< Key pressed */
    KEYUP,                  /**< Key released */
    SDL_TEXTEDITING,            /**< Keyboard text editing (composition) */
    SDL_TEXTINPUT,              /**< Keyboard text input */

    /* Mouse events */
    SDL_MOUSEMOTION = 0x400, /**< Mouse moved */
    SDL_MOUSEBUTTONDOWN,        /**< Mouse button pressed */
    SDL_MOUSEBUTTONUP,          /**< Mouse button released */
    SDL_MOUSEWHEEL,             /**< Mouse wheel motion */

    /* Tablet or multiple mice input device events */
    SDL_INPUTMOTION = 0x500, /**< Input moved */
    SDL_INPUTBUTTONDOWN,        /**< Input button pressed */
    SDL_INPUTBUTTONUP,          /**< Input button released */
    SDL_INPUTWHEEL,             /**< Input wheel motion */
    SDL_INPUTPROXIMITYIN,       /**< Input pen entered proximity */
    SDL_INPUTPROXIMITYOUT,      /**< Input pen left proximity */

    /* Joystick events */
    SDL_JOYAXISMOTION = 0x600, /**< Joystick axis motion */
    SDL_JOYBALLMOTION,          /**< Joystick trackball motion */
    SDL_JOYHATMOTION,           /**< Joystick hat position change */
    SDL_JOYBUTTONDOWN,          /**< Joystick button pressed */
    SDL_JOYBUTTONUP,            /**< Joystick button released */

    /* Touch events */
    SDL_FINGERDOWN = 0x700,
    SDL_FINGERUP,
    SDL_FINGERMOTION,
    SDL_TOUCHBUTTONDOWN,
    SDL_TOUCHBUTTONUP,

    /* Gesture events */
    SDL_DOLLARGESTURE = 0x800,
    SDL_DOLLARRECORD,
    SDL_MULTIGESTURE,

    /* Clipboard events */

    SDL_CLIPBOARDUPDATE = 0x900, /**< The clipboard changed */

    /* Obsolete events */
    SDL_EVENT_COMPAT1 = 0x7000, /**< SDL 1.2 events for compatibility */
    SDL_EVENT_COMPAT2,
    SDL_EVENT_COMPAT3,


    /** Events ::SDL_USEREVENT through ::SDL_LASTEVENT are for your use,
     *  and should be allocated with SDL_RegisterEvents()
     */
    SDL_USEREVENT = 0x8000,

    /**
     *  This last event is only for bounding internal arrays
     */
    SDL_LASTEVENT = 0xFFFF
}