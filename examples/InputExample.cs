namespace Pollus.Game;

using Pollus.Engine;
using Pollus.Engine.Input;

public class InputExample
{
    Keyboard? keyboard = null;

    public void Setup(IApplication app)
    {
        keyboard = app.Input.GetDevice("keyboard") as Keyboard;
    }

    public void Update(IApplication app)
    {
        if (keyboard!.JustPressed(Key.ArrowLeft))
        {
            Console.WriteLine("Arrow Left Just Pressed");
        }
        else if (keyboard!.Pressed(Key.ArrowLeft))
        {
            Console.WriteLine("Arrow Left Pressed");
        }
        else if (keyboard!.JustReleased(Key.ArrowLeft))
        {
            Console.WriteLine("Arrow Left Just Released");
        }
    }

    public void Run()
    {
        (ApplicationBuilder.Default with
        {
            OnSetup = Setup,
            OnUpdate = Update
        }).Build().Run();
    }
}