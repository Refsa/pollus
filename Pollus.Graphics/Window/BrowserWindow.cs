namespace Pollus.Graphics.Windowing;

using Pollus.Mathematics;
using Pollus.Emscripten;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Contexts;

public partial class BrowserWindow : IWindow
{
    static BrowserWindow instance;
    static Action emOnFrame;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void emOnFrameCallback()
    {
        emOnFrame();
    }

    public bool IsOpen { get; private set; }
    public WindowOptions Options { get; private set; }
    public Vector2<int> Size { get; set; }
    public INativeWindow? Native => null;

    nint nativeWindow;

    public BrowserWindow(WindowOptions options)
    {
        instance = this;
        Options = options;
        Size = new Vector2<int>(options.Width, options.Height);

        nativeWindow = EmscriptenSDL.CreateWindow(options.Title,
            Silk.NET.SDL.Sdl.WindowposUndefined, Silk.NET.SDL.Sdl.WindowposUndefined,
            options.Width, options.Height,
            Silk.NET.SDL.WindowFlags.InputFocus | Silk.NET.SDL.WindowFlags.Resizable);

        IsOpen = true;
    }

    public void Dispose()
    {
        if (IsOpen is false) return;
        IsOpen = false;

        EmscriptenSDL.DestroyWindow(nativeWindow);
    }

    public void Close()
    {
        IsOpen = false;
    }

    unsafe public void Run(Action loop)
    {
        emOnFrame = loop;
        Emscripten.SetMainLoop(&emOnFrameCallback, 0, false);
    }
}