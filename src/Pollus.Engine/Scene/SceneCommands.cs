namespace Pollus.Engine;

using Assets;
using ECS;
using Utils;

public static class SceneCommands
{
    public static EntityCommands SpawnScene(this Commands commands, in Handle<Scene> scene)
    {
        var root = commands.Spawn();
        commands.AddCommand(new SpawnSceneCommand { Scene = scene, Root = root.Entity });
        return root;
    }
}

public struct SpawnSceneCommand : ICommand
{
    public static int Priority => 99;

    public required Handle<Scene> Scene;
    public required Entity Root;

    public void Execute(World world)
    {
        var assetServer = world.Resources.Get<AssetServer>();
        var sceneAssets = assetServer.GetAssets<Scene>();
        var scene = sceneAssets.Get(Scene);
        if (scene is null) throw new Exception($"Scene {Scene} not found");

        world.Store.AddComponent(Root, new SceneRoot());
        world.Store.AddComponent(Root, new SceneRef { Scene = Scene });

        foreach (var sceneEntity in scene.Entities)
        {
            var entity = SpawnEntity(world, sceneAssets, sceneEntity);
            new AddChildCommand { Parent = Root, Child = entity }.Execute(world);
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