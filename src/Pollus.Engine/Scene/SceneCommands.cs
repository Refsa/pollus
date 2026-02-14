namespace Pollus.Engine;

using Pollus.Assets;
using ECS;
using Utils;

public static class SceneCommands
{
    public static EntityCommands SpawnScene(this Commands commands, in Handle<Scene> scene)
    {
        var root = commands.Spawn(Entity.With(new PendingSceneLoad { Scene = scene }));
        return root;
    }
}