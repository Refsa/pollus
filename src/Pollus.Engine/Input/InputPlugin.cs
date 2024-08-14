namespace Pollus.Engine.Input;

using Pollus.ECS;

public class InputPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(InputManager.Create());
    }
}