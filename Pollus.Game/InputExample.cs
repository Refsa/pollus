namespace Pollus.Game;

using Pollus.Engine;
using Pollus.Engine.Input;

public class InputExample
{
    Input? input = null;
    Keyboard? keyboard = null;

    public void Setup(IApplication app)
    {
        input = Input.Create();
        keyboard = input.GetDevice("keyboard") as Keyboard;
    }

    public void Update(IApplication app)
    {
        input?.Update();
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