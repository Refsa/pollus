namespace Pollus.Graphics;

using Silk.NET.Core.Contexts;
using Pollus.Mathematics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public record class WindowOptions
{
    public string Title { get; set; } = "Pollus";
    public int Width { get; set; } = 1600;
    public int Height { get; set; } = 900;
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public bool VSync { get; set; } = false;
    public int FramesPerSecond { get; set; } = 144;
}

public partial class Window : IDisposable, INativeWindowSource
{
#if BROWSER
    static Window instance;
    static Action emOnFrame;

    [DllImport("__Internal_emscripten")]
    private static extern void emscripten_set_main_loop(IntPtr action, int fps, bool simulateInfiniteLoop);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static void emOnFrameCallback()
    {
        emOnFrame();
    }
#endif

    bool isOpen;
    INativeWindow? window;

    public bool IsOpen => isOpen;

    public INativeWindow? Native => window;

    public Vector2<int> Size { get; private set; }

    public Window(WindowOptions options)
    {
#if BROWSER
        instance = this;
#else
        window = SDLWrapper.CreateWindow(options);
        Size = new Vector2<int>(options.Width, options.Height);
#endif

        isOpen = true;
    }

    public void Dispose()
    {
        if (isOpen is false) return;
        isOpen = false;

#if !BROWSER
        SDLWrapper.DestroyWindow(window);
#endif
    }

    public void PollEvents()
    {
#if !BROWSER
        foreach (var @event in SDLWrapper.PollEvents())
        {
            switch (@event.Type)
            {
                case WindowEventType.Closed:
                    isOpen = false;
                    break;
            }
        }
#endif
    }

    unsafe public void Run(Action loop)
    {
#if BROWSER
        emOnFrame = loop;
        emscripten_set_main_loop((IntPtr)(delegate* unmanaged[Cdecl]<void>)&emOnFrameCallback, 0, false);
#else
        while (IsOpen)
        {
            PollEvents();
            loop();
        }
#endif
    }
}

public enum WindowEventType
{
    Closed,
}

[StructLayout(LayoutKind.Explicit)]
public struct WindowEvent
{
    [FieldOffset(0)]
    public readonly WindowEventType Type;

    public WindowEvent(WindowEventType type)
    {
        Type = type;
    }

    public static implicit operator WindowEvent(WindowEventType type) => new(type);
}