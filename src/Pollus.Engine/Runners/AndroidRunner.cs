namespace Pollus.Engine.Runners;

using Pollus.ECS;

public class AndroidRunner : IAppRunner
{
    public bool IsBlocking => true;

    public void Setup(World world) { }

    public void Run(World world, Func<bool> isRunning, Action requestShutdown)
    {
        throw new NotImplementedException("Android runner not yet implemented.");
    }
}
