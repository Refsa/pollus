namespace Pollus.Graphics;

using Silk.NET.Core.Contexts;
using Pollus.ECS;

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

public class Window : IDisposable
{
    bool isOpen;
    INativeWindow window;

    public bool IsOpen => isOpen;

    public Window(WindowOptions options)
    {
        isOpen = true;
        window = SDL.CreateWindow(options);
    }

    public void Dispose()
    {
        isOpen = false;
        SDL.DestroyWindow(window);
    }
}

public struct WindowEvent<T>
{
    public T Event { get; set; }
}