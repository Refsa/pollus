namespace Pollus.Game;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Input;

public class InputExample
{
    Keyboard? keyboard = null;

    public void Setup(IApplication app)
    {
        var inputManager = app.World.Resources.Get<InputManager>();
        keyboard = inputManager.GetDevice("keyboard") as Keyboard;
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
            World = new World().AddPlugin<InputPlugin>(),
            OnSetup = Setup,
            OnUpdate = Update
        }).Build().Run();
    }
}