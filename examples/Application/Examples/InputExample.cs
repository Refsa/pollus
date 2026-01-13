namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Input;

public class InputExample : IExample
{
    public string Name => "input";
    IApplication? application;

    public void Stop() => application?.Shutdown();

    public void Run()
    {
        application = Application.Builder
            .AddPlugin<InputPlugin>()
            .AddSystem(CoreStage.Update, FnSystem.Create("Update",
                static (EventReader<ButtonEvent<Key>> eKeys, ButtonInput<Key> directInput) =>
                {
                    foreach (var key in eKeys.Read())
                    {
                        Log.Info($"Key {key.Button} {(key.State == ButtonState.JustReleased ? "released" : "pressed")}");
                    }
                }))
            .Build();
        application.Run();
    }
}