namespace Pollus.Engine.Rendering;

using Pollus.ECS;

public class SpriteAnimatorPlugin : IPlugin
{
    public static SpriteAnimatorPlugin Default => new();

    public void Apply(World world)
    {
        world.Events.InitEvent<SpriteAnimatorEvents.ClipChangeRequest>();
        world.Events.InitEvent<SpriteAnimatorEvents.ClipStarted>();
        world.Events.InitEvent<SpriteAnimatorEvents.ClipEnded>();
        world.Events.InitEvent<SpriteAnimatorEvents.ClipFrame>();

        world.Schedule.AddSystemSet<SpriteAnimationSystems>();
    }
}
