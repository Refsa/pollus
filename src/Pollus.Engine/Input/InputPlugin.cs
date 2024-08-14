namespace Pollus.Engine.Input;

using Pollus.ECS;
using static Pollus.ECS.SystemBuilder;

public class InputPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(InputManager.Create());
        world.Schedule.AddSystem(CoreStage.First, FnSystem("InputUpdate",
        (InputManager input) =>
        {
            input.Update();
        }));
    }
}