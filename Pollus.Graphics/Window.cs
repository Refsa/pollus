namespace Pollus.Graphics;

using Silk.NET.Core.Contexts;
using Pollus.ECS;
using Pollus.Mathematics;
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

public class Window : IDisposable, INativeWindowSource
{
    bool isOpen;
    INativeWindow window;

    public bool IsOpen => isOpen;

    public INativeWindow? Native => window;

    public Vector2<int> Size { get; private set; }

    public Window(WindowOptions options)
    {
        isOpen = true;
        window = SDL.CreateWindow(options);
        Size = new Vector2<int>(options.Width, options.Height);
    }

    public void Dispose()
    {
        isOpen = false;
        SDL.DestroyWindow(window);
    }

    public void PollEvents()
    {
        foreach (var @event in SDL.PollEvents())
        {
            switch (@event.Type)
            {
                case WindowEventType.Closed:
                    isOpen = false;
                    break;
            }
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