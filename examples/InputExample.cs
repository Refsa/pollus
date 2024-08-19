namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.ECS.Core;
using Pollus.Engine;
using Pollus.Engine.Input;
using static Pollus.ECS.SystemBuilder;

public class InputExample
{
    public void Run()
    {
        Application.Builder
            .AddPlugin<InputPlugin>()
            .AddSystem(CoreStage.Update, FnSystem("Update",
            static (InputManager inputManager) =>
            {
                var keyboard = inputManager.GetDevice("keyboard") as Keyboard;

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
            }))
            .Run();
    }
}