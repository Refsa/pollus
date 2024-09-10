namespace Pollus.Graphics.Windowing;

using Silk.NET.Core.Contexts;
using Pollus.Mathematics;
using Pollus.Graphics.SDL;

public class DesktopWindow : IWindow, INativeWindowSource
{
    INativeWindow native;

    public bool IsOpen { get; private set; }
    public WindowOptions Options { get; private set; }
    public Vec2<uint> Size { get; set; }

    public INativeWindow? Native => native;

    public DesktopWindow(WindowOptions options)
    {
        Options = options;
        Size = new Vec2<uint>(options.Width, options.Height);
        native = SDLWrapper.CreateWindow(options);
        IsOpen = true;
    }

    public void Dispose()
    {
        if (IsOpen is false) return;
        IsOpen = false;
        SDLWrapper.DestroyWindow(native);
    }

    public void Close()
    {
        IsOpen = false;
    }

    public void Run(Action loop)
    {
        while (IsOpen)
        {
            loop();
        }
    }
}
