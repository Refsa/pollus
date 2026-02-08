namespace Pollus.Engine.Runners;

using Pollus.ECS;

public class HeadlessRunner : IAppRunner
{
    public bool IsBlocking => true;

    public void Setup(World world) { }

    public void Run(World world, Func<bool> isRunning, Action requestShutdown)
    {
        while (isRunning())
        {
            world.Update();
        }
    }
}
