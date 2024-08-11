using Pollus.Engine;

namespace Pollus.Game;

public class SnakeGame
{
    public void Run()
    {
        (ApplicationBuilder.Default with
        {
            OnSetup = Setup,
            OnUpdate = Update,
        }).Build().Run();
    }

    public void Setup(IApplication app)
    {
    }

    public void Update(IApplication app)
    {
    }
}