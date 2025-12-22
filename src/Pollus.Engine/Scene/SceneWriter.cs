namespace Pollus.Engine;

using System.Text.Json;
using System.Collections.Generic;
using Assets;
using Core.Serialization;
using ECS;
using Serialization;
using Utils;

public ref struct SceneWriter : IWriter, IDisposable
{
    public struct Options()
    {
        public static Options Default => new();

        public int FormatVersion { get; set; } = 1;
        public int TypesVersion { get; set; } = 1;

        public bool Indented { get; set; } = false;
        public bool WriteRoot { get; set; } = true;
        public bool WriteSubScenes { get; set; } = false;
    }

    static HashSet<ComponentID> ignoredComponents = new()
    {
        Component.GetInfo<Parent>().ID,
        Component.GetInfo<Child>().ID,
        Component.GetInfo<SceneRef>().ID,
        Component.GetInfo<SceneRoot>().ID,
    };

    SceneFileData data;
    DefaultSerializationContext defaultContext;
    WorldSerializationContext context;

    readonly Options options;

    Dictionary<string, JsonElement> currentProperties;
    JsonElement currentValue;

    public ReadOnlySpan<byte> Buffer => throw new NotSupportedException();

    public SceneWriter()
    {
        this.currentProperties = Pool<Dictionary<string, JsonElement>>.Shared.Rent();
    }

    public SceneWriter(Options options) : this()
    {
        this.options = options;
    }

    public void Dispose()
    {
        currentProperties.Clear();
        Pool<Dictionary<string, JsonElement>>.Shared.Return(currentProperties);
    }

    public byte[] Write(in World world, in Entity root)
    {
        this.context = new() { AssetServer = world.Resources.Get<AssetServer>() };
        this.defaultContext = new();

        data = new()
        {
            FormatVersion = options.FormatVersion,
            TypesVersion = options.TypesVersion,
            Entities = [],
            Types = [],
        };

        if (options.WriteRoot) data.Entities.Add(CollectEntityData(world, root));
        else
        {
            var entityRef = world.GetEntityRef(root);
            ref var parent = ref entityRef.TryGet<Parent>(out var parentExists);
            if (!parentExists) throw new Exception($"Entity {root} has no parent");

            var currentEntity = parent.FirstChild;
            while (currentEntity != Entity.NULL)
            {
                data.Entities.Add(CollectEntityData(world, currentEntity));
                currentEntity = world.GetEntityRef(currentEntity).Get<Child>().NextSibling;
            }
        }

        var serializer = options switch
        {
            { Indented: true } => SceneFileDataJsonSerializerContext.Indented,
            _ => SceneFileDataJsonSerializerContext.Default,
        };
        return JsonSerializer.SerializeToUtf8Bytes(data, serializer.SceneFileData);
    }

    SceneFileData.EntityData CollectEntityData(in World world, in Entity entity)
    {
        var entityData = new SceneFileData.EntityData()
        {
            ID = entity.ID,
            Components = [],
            Children = [],
        };

        var entityInfo = world.Store.GetEntityInfo(entity);
        var archetype = world.Store.GetArchetype(entityInfo.ArchetypeIndex);
        ref var chunk = ref archetype.Chunks[entityInfo.ChunkIndex];

        if (archetype.HasComponent<SceneRef>())
        {
            ref var sceneRef = ref chunk.GetComponent<SceneRef>(entityInfo.RowIndex);
            if (!options.WriteSubScenes)
            {
                var assetPath = world.Resources.Get<AssetServer>().GetAssets<Scene>().GetPath(sceneRef.Scene);
                if (assetPath.HasValue) entityData.Scene = assetPath.Value.Path;
                return entityData;
            }
        }

        if (archetype.HasComponent<Parent>())
        {
            ref var parent = ref chunk.GetComponent<Parent>(entityInfo.RowIndex);
            var current = parent.FirstChild;
            while (current != Entity.NULL)
            {
                entityData.Children.Add(CollectEntityData(world, current));
                current = world.GetEntityRef(current).Get<Child>().NextSibling;
            }
        }

        foreach (var componentId in archetype.GetChunkInfo().ComponentIDs)
        {
            if (ignoredComponents.Contains(componentId)) continue;

            var cinfo = Component.GetInfo(componentId);
            var alias = cinfo.Type.Name;
            if (!data.Types.ContainsKey(alias))
            {
                data.Types.Add(alias, cinfo.TypeName);
            }

            currentProperties.Clear();
            currentValue = default;

            if (BlittableSerializerLookup<WorldSerializationContext>.GetSerializer(cinfo.Type) is { } serializer)
            {
                serializer.SerializeBytes(ref this, chunk.GetComponent(entityInfo.RowIndex, componentId), in context);
            }
            else if (BlittableSerializerLookup<DefaultSerializationContext>.GetSerializer(cinfo.Type) is { } defaultSerializer)
            {
                defaultSerializer.SerializeBytes(ref this, chunk.GetComponent(entityInfo.RowIndex, componentId), in defaultContext);
            }
            else
            {
                throw new InvalidOperationException($"No serializer found for component {cinfo.Type}");
            }

            if (currentProperties.Count > 0)
            {
                entityData.Components.Add(alias, JsonSerializer.SerializeToElement(currentProperties));
            }
            else if (currentValue.ValueKind != JsonValueKind.Undefined)
            {
                entityData.Components.Add(alias, currentValue);
            }
            else
            {
                entityData.Components.Add(alias, JsonSerializer.SerializeToElement(new object()));
            }
        }

        return entityData;
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void Write<T>(T value, string? identifier = null) where T : unmanaged
    {
        if (BlittableSerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            var prevProperties = currentProperties;
            var prevValue = currentValue;
            currentProperties = new();
            currentValue = default;

            serializer.Serialize(ref this, value, in context);

            JsonElement result;
            if (currentProperties.Count > 0) result = JsonSerializer.SerializeToElement(currentProperties);
            else if (currentValue.ValueKind != JsonValueKind.Undefined) result = currentValue;
            else result = JsonSerializer.SerializeToElement(new object());

            currentProperties = prevProperties;
            currentValue = prevValue;

            if (identifier != null) currentProperties![identifier] = result;
            else currentValue = result;
            return;
        }

        if (BlittableSerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
        {
            var prevProperties = currentProperties;
            var prevValue = currentValue;
            currentProperties = new();
            currentValue = default;

            defaultSerializer.Serialize(ref this, value, in defaultContext);

            JsonElement result;
            if (currentProperties.Count > 0) result = JsonSerializer.SerializeToElement(currentProperties);
            else if (currentValue.ValueKind != JsonValueKind.Undefined) result = currentValue;
            else result = JsonSerializer.SerializeToElement(new object());

            currentProperties = prevProperties;
            currentValue = prevValue;

            if (identifier != null) currentProperties[identifier] = result;
            else currentValue = result;
            return;
        }

        var element = JsonSerializer.SerializeToElement(value);
        if (identifier != null) currentProperties[identifier] = element;
        else currentValue = element;
    }

    public void Write<T>(T[] values, string? identifier = null) where T : unmanaged
    {
        var element = JsonSerializer.SerializeToElement(values);
        if (identifier != null) currentProperties[identifier] = element;
        else currentValue = element;
    }

    public void Write(scoped ReadOnlySpan<byte> data, string? identifier = null)
    {
        throw new NotImplementedException();
    }

    public void Write<T>(scoped ReadOnlySpan<T> data, string? identifier = null) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void Write(string value, string? identifier = null)
    {
        var element = JsonSerializer.SerializeToElement(value);
        if (identifier != null) currentProperties[identifier] = element;
        else currentValue = element;
    }

    public void Serialize<T>(in T value, string? identifier = null) where T : notnull
    {
        var prevProperties = currentProperties;
        var prevValue = currentValue;

        currentProperties = new();
        currentValue = default;

        bool serialized = false;
        if (SerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            serializer.Serialize(ref this, value, in context);
            serialized = true;
        }
        else if (SerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
        {
            defaultSerializer.Serialize(ref this, value, in defaultContext);
            serialized = true;
        }

        JsonElement result;
        if (serialized)
        {
            result = currentProperties.Count > 0 ? JsonSerializer.SerializeToElement(currentProperties) : currentValue;
        }
        else
        {
            result = JsonSerializer.SerializeToElement(value);
        }

        currentProperties = prevProperties;
        currentValue = prevValue;

        if (identifier != null) currentProperties[identifier] = result;
        else currentValue = result;
    }
}
