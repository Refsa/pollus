namespace Pollus.Graphics.Windowing;

using Pollus.Mathematics;
using Silk.NET.Core.Contexts;
using System.Runtime.InteropServices;

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

public interface IWindow : IDisposable, INativeWindowSource
{
    bool IsOpen { get; }
    WindowOptions Options { get; }
    Vector2<int> Size { get; set; }

    public void Run(Action loop);
}

public static class Window
{
    public static IWindow Create(WindowOptions options)
    {
#if BROWSER
        return new BrowserWindow(options);
#else
        return new DesktopWindow(options);
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