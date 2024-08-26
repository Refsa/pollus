namespace Pollus.Examples;

using Pollus.Debugging;
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
            static (EventReader<ButtonEvent<Key>> eKeys) =>
            {
                foreach (var key in eKeys.Read())
                {
                    Log.Info($"Key {key.Button} {(key.State == ButtonState.JustReleased ? "released" : "pressed")}");
                }
            }))
            .Run();
    }
}