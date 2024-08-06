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

public class Window : IDisposable, INativeWindowSource
{
#if NET8_0_BROWSER
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
#if !NET8_0_BROWSER
        window = SDL.CreateWindow(options);
        Size = new Vector2<int>(options.Width, options.Height);
#else
        instance = this;
#endif

        isOpen = true;
    }

    public void Dispose()
    {
        if (isOpen is false) return;
        isOpen = false;

#if !NET8_0_BROWSER
        SDL.DestroyWindow(window);
#endif
    }

    public void PollEvents()
    {
#if !NET8_0_BROWSER
        foreach (var @event in SDL.PollEvents())
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
#if NET8_0_BROWSER
        emOnFrame = loop;
        emscripten_set_main_loop((IntPtr)(delegate* unmanaged[Cdecl]<void>)&emOnFrameCallback, 0, false);
#endif

        while (IsOpen)
        {
            loop();
            PollEvents();
        }
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