namespace Pollus.Engine.Input;

using Pollus.ECS;
using Pollus.Engine.Platform;
using static Pollus.ECS.SystemBuilder;

public class InputPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(InputManager.Create());
        world.Schedule.AddSystems(CoreStage.First, FnSystem("InputUpdate",
        (InputManager input, PlatformEvents platform) =>
        {
            input.Update(platform);
        }));
    }
}