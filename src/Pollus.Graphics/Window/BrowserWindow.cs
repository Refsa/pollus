#pragma warning disable CS8618

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
    public Vec2<uint> Size { get; set; }
    public INativeWindow? Native => null;

    nint nativeWindow;

    public BrowserWindow(WindowOptions options)
    {
        instance = this;
        Options = options;
        Size = new Vec2<uint>(options.Width, options.Height);

        EmscriptenSDL.Init(SDLInitFlags.InitVideo | SDLInitFlags.InitJoystick);
        nativeWindow = EmscriptenSDL.CreateWindow(options.Title,
            Silk.NET.SDL.Sdl.WindowposUndefined, Silk.NET.SDL.Sdl.WindowposUndefined,
            (int)options.Width, (int)options.Height,
            Silk.NET.SDL.WindowFlags.Resizable);

        IsOpen = true;
    }

    public void Dispose()
    {
        if (IsOpen is false) return;
        IsOpen = false;

        Emscripten.ClearMainLoop();
        EmscriptenSDL.DestroyWindow(nativeWindow);
        EmscriptenSDL.Quit();
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

    public void SetTitle(string title)
    {
        
    }
}

#pragma warning restore CS8618