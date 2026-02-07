namespace Pollus.Engine;

using System.Runtime.InteropServices;
using Assets;
using Debugging;
using ECS;
using Utils;

public static class SceneHelper
{
    public static void SpawnScene(Commands commands, Assets<Scene> sceneAssets, Entity rootEntity, Handle<Scene> sceneHandle)
    {
        var assetInfo = sceneAssets.GetInfo(sceneHandle);
        if (assetInfo is null) throw new Exception($"Scene {sceneHandle} not found");

        if (assetInfo.Status != AssetStatus.Loaded || assetInfo.Asset is null)
        {
            commands.AddComponent(rootEntity, new PendingSceneLoad { Scene = sceneHandle });
            return;
        }

        var scene = sceneAssets.Get(sceneHandle);
        Guard.IsNotNull(scene, (FormattableString)$"Scene {sceneHandle} not found");

        commands.AddComponent(rootEntity, new SceneRoot());
        commands.AddComponent(rootEntity, new SceneRef { Scene = sceneHandle });

        foreach (var sceneEntity in scene.Entities)
        {
            var entity = SpawnEntity(commands, sceneAssets, sceneEntity);
            commands.AddChild(rootEntity, entity.Entity);
        }
    }

    static EntityCommands SpawnEntity(Commands commands, Assets<Scene> sceneAssets, in Scene.SceneEntity sceneEntity)
    {
        var entityCommands = commands.Spawn();
        if (sceneEntity.Components is { Count: > 0 })
        {
            var components = CollectionsMarshal.AsSpan(sceneEntity.Components);
            foreach (scoped ref readonly var t in components)
            {
                entityCommands.AddComponent(t.ComponentID, t.Data);
            }

            foreach (scoped ref readonly var t in components)
            {
                if (!RequiredComponents.TryGet(t.ComponentID, out var required)) continue;

                foreach (var kvp in required.Defaults)
                {
                    if (kvp.Key == t.ComponentID) continue;

                    bool alreadyInScene = false;
                    foreach (scoped ref readonly var s in components)
                    {
                        if (s.ComponentID == kvp.Key) { alreadyInScene = true; break; }
                    }
                    if (alreadyInScene) continue;

                    entityCommands.AddComponent(kvp.Key, kvp.Value);
                }
            }
        }

        if (sceneEntity.Children is not null)
        {
            foreach (var sceneEntityChild in sceneEntity.Children)
            {
                var childEntity = SpawnEntity(commands, sceneAssets, sceneEntityChild);
                entityCommands.AddChild(childEntity.Entity);
            }
        }

        if (sceneEntity.Scene is { } childSceneHandle)
        {
            SpawnScene(commands, sceneAssets, entityCommands.Entity, childSceneHandle);
        }

        return entityCommands;
    }
}