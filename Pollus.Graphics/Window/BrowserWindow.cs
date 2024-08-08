namespace Pollus.Graphics.Windowing;

using Pollus.Mathematics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Contexts;

public partial class BrowserWindow : IWindow
{
    static BrowserWindow instance;
    static Action emOnFrame;

    [DllImport("__Internal_emscripten")]
    private static extern void emscripten_set_main_loop(nint action, int fps, bool simulateInfiniteLoop);

    [DllImport("__Internal_emscripten")]
    private static extern void emscripten_request_animation_frame(nint callback, nint userData);

    [DllImport("__Internal_emscripten")]
    private static extern void emscripten_set_timeout(nint callback, double ms, nint userData);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    async static void emOnFrameCallback()
    {
        emOnFrame();
    }

    public bool IsOpen { get; private set; }
    public WindowOptions Options { get; private set; }
    public Vector2<int> Size { get; set; }
    public INativeWindow? Native => null;

    public BrowserWindow(WindowOptions options)
    {
        instance = this;
        Options = options;
        Size = new Vector2<int>(options.Width, options.Height);
        IsOpen = true;
    }

    public void Dispose()
    {
        if (IsOpen is false) return;
        IsOpen = false;
    }

    unsafe public void Run(Action loop)
    {
        emOnFrame = loop;
        emscripten_set_main_loop((IntPtr)(delegate* unmanaged[Cdecl]<void>)&emOnFrameCallback, 0, false);
    }

    unsafe public static void RequestAnimationFrame(delegate* unmanaged[Cdecl]<double, void*, void> callback, void* userData)
    {
        emscripten_request_animation_frame((nint)callback, (nint)userData);
    }

    unsafe public static void SetTimeout(delegate* unmanaged[Cdecl]<void*, void> callback, void* userData, double ms)
    {
        emscripten_set_timeout((nint)callback, ms, (nint)userData);
    }
}
