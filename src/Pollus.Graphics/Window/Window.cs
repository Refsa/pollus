namespace Pollus.Graphics.Windowing;

using Pollus.Mathematics;
using Silk.NET.Core.Contexts;

public enum WindowPositioning
{
    Default,
    Centered,
    Position,
}

public record class WindowOptions
{
    public static WindowOptions Default => new();

    public string Title { get; set; } = "Pollus";
    public int? TargetDisplay { get; set; }
    public WindowPositioning Positioning { get; set; }
    public uint Width { get; set; } = 1600;
    public uint Height { get; set; } = 900;
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
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
        if (OperatingSystem.IsBrowser()) return new BrowserWindow(options);
        return new DesktopWindow(options);
    }
}