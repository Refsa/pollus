namespace Pollus.Engine;

using Assets;
using Debugging;
using ECS;
using Utils;

public static class SceneHelper
{
    public static void SpawnScene(World world, Assets<Scene> sceneAssets, Entity root, Handle<Scene> sceneHandle)
    {
        var scene = sceneAssets.Get(sceneHandle);
        Guard.IsNotNull(scene, (FormattableString)$"Scene {sceneHandle} not found");

        world.Store.AddComponent(root, new SceneRoot());
        world.Store.AddComponent(root, new SceneRef { Scene = sceneHandle });

        foreach (var sceneEntity in scene.Entities)
        {
            var entity = SpawnEntity(world, sceneAssets, sceneEntity);
            new AddChildCommand { Parent = root, Child = entity }.Execute(world);
        }
    }

    static Entity SpawnEntity(World world, Assets<Scene> sceneAssets, in Scene.SceneEntity entity)
    {
        ArchetypeStore.EntityChange entityRef;
        if (entity.Components is { Count: > 0 })
        {
            Span<ComponentID> cids = stackalloc ComponentID[entity.Components.Count];
            for (int i = 0; i < entity.Components.Count; i++)
            {
                cids[i] = entity.Components[i].ComponentID;
            }

            var aid = ArchetypeID.Create(cids);
            entityRef = world.Store.CreateEntity(aid, cids);

            ref var chunk = ref entityRef.Archetype.Chunks[entityRef.ChunkIndex];
            for (int i = 0; i < entity.Components.Count; i++)
            {
                chunk.SetComponent(entityRef.RowIndex, entity.Components[i].ComponentID, entity.Components[i].Data);
            }
        }
        else
        {
            entityRef = world.Store.CreateEntity<EntityBuilder>();
        }

        if (entity.Children is not null)
        {
            foreach (var sceneEntityChild in entity.Children)
            {
                var childEntity = SpawnEntity(world, sceneAssets, sceneEntityChild);
                new AddChildCommand { Parent = entityRef.Entity, Child = childEntity }.Execute(world);
            }
        }

        if (entity.Scene is not null)
        {
            var scene = sceneAssets.Get(entity.Scene.Value);
            if (scene is null) throw new Exception($"Scene {entity.Scene.Value} not found");
            var spawnSceneCommand = new SpawnSceneCommand { Scene = entity.Scene.Value, Root = entityRef.Entity };
            spawnSceneCommand.Execute(world);
        }

        return entityRef.Entity;
    }
}