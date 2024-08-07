namespace Pollus.Graphics.Windowing;

using Silk.NET.Core.Contexts;
using Pollus.Mathematics;

public class DesktopWindow : IWindow, INativeWindowSource
{
    INativeWindow native;

    public bool IsOpen { get; private set; }
    public WindowOptions Options { get; private set; }
    public Vector2<int> Size { get; set; }

    public INativeWindow? Native => native;

    public DesktopWindow(WindowOptions options)
    {
        Options = options;
        Size = new Vector2<int>(options.Width, options.Height);
        native = SDLWrapper.CreateWindow(options);
        IsOpen = true;
    }

    public void Dispose()
    {
        if (IsOpen is false) return;
        IsOpen = false;
        SDLWrapper.DestroyWindow(native);
    }

    public void Run(Action loop)
    {
        while (IsOpen)
        {
            PollEvents();
            loop();
        }
    }

    public void PollEvents()
    {
        foreach (var @event in SDLWrapper.PollEvents())
        {
            switch (@event.Type)
            {
                case WindowEventType.Closed:
                    IsOpen = false;
                    break;
            }
        }
    }
}
