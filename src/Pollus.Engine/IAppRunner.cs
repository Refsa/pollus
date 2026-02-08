namespace Pollus.Engine;

using Pollus.ECS;

public interface IAppRunner
{
    bool IsBlocking { get; }
    void Setup(World world);
    void Run(World world, Func<bool> isRunning, Action requestShutdown);
}
