namespace Pollus.Graphics.Windowing;

using Pollus.Mathematics;
using Silk.NET.Core.Contexts;
using System.Runtime.InteropServices;

public record class WindowOptions
{
    public static WindowOptions Default => new();

    public string Title { get; set; } = "Pollus";
    public uint Width { get; set; } = 1600;
    public uint Height { get; set; } = 900;
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public bool VSync { get; set; } = false;
    public int FramesPerSecond { get; set; } = 144;
    public bool Resizeable { get; set; } = true;
    public bool Fullscreen { get; set; } = false;
    public bool Borderless { get; set; } = false;
    public bool MouseCapture { get; set; } = false;
}

public interface IWindow : IDisposable, INativeWindowSource
{
    bool IsOpen { get; }
    WindowOptions Options { get; }
    Vec2<uint> Size { get; set; }

    public void Run(Action loop);

    public void SetTitle(string title);
    public void HideCursor();
    public void ShowCursor();

    public void Close();
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