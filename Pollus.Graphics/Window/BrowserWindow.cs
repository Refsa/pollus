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
    private static extern void emscripten_set_main_loop(IntPtr action, int fps, bool simulateInfiniteLoop);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static void emOnFrameCallback()
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
}
