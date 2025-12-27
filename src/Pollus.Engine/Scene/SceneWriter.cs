namespace Pollus.Engine;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Assets;
using Core.Serialization;
using ECS;
using Pollus.Debugging;
using Serialization;

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

    DefaultSerializationContext defaultContext;
    WorldSerializationContext context;

    readonly Options options;

    readonly MemoryStream stream;
    readonly Utf8JsonWriter writer;

    public ReadOnlySpan<byte> Buffer => stream.GetBuffer();

    public SceneWriter(Options options) : this()
    {
        this.options = options;
        stream = new();
        writer = new(stream, new JsonWriterOptions() { Indented = options.Indented });
    }

    public void Dispose()
    {
        writer?.Dispose();
    }

    public byte[] Write(in World world, in Entity root)
    {
        this.context = new() { AssetServer = world.Resources.Get<AssetServer>() };
        this.defaultContext = new();

        writer.WriteStartObject();

        writer.WriteNumber("FormatVersion", options.FormatVersion);
        writer.WriteNumber("TypesVersion", options.TypesVersion);

        {
            writer.WriteStartObject("Types");

            var types = new HashSet<Type>();
            CollectTypes(world, root, types);
            foreach (var type in types)
            {
                writer.WriteString(type.Name, type.AssemblyQualifiedName);
            }

            writer.WriteEndObject();
        }

        {
            writer.WriteStartArray("Entities");

            if (options.WriteRoot) WriteEntityData(world, root);
            else
            {
                var entityRef = world.GetEntityRef(root);
                ref var parent = ref entityRef.TryGet<Parent>(out var parentExists);
                if (!parentExists) throw new Exception($"Entity {root} has no parent");

                var currentEntity = parent.FirstChild;
                while (currentEntity != Entity.NULL)
                {
                    WriteEntityData(world, currentEntity);
                    currentEntity = world.GetEntityRef(currentEntity).Get<Child>().NextSibling;
                }
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
        writer.Flush();
        return stream.ToArray();
    }

    void CollectTypes(in World world, in Entity entity, HashSet<Type> types)
    {
        var entityInfo = world.Store.GetEntityInfo(entity);
        var archetype = world.Store.Archetypes[entityInfo.ArchetypeIndex];
        foreach (var cid in archetype.GetChunkInfo().ComponentIDs)
        {
            var cinfo = Component.GetInfo(cid);
            if (ignoredComponents.Contains(cid)) continue;
            types.Add(cinfo.Type);
        }

        if (archetype.HasComponent<SceneRef>() && !options.WriteSubScenes)
        {
            return;
        }

        if (archetype.HasComponent<Parent>())
        {
            var parent = archetype.Chunks[entityInfo.ChunkIndex].GetComponent<Parent>(entityInfo.RowIndex);
            var child = parent.FirstChild;
            while (child != Entity.NULL)
            {
                CollectTypes(world, child, types);
                child = world.GetEntityRef(child).Get<Child>().NextSibling;
            }
        }
    }

    void WriteEntityData(in World world, in Entity entity)
    {
        writer.WriteStartObject();

        var entityInfo = world.Store.GetEntityInfo(entity);
        var archetype = world.Store.GetArchetype(entityInfo.ArchetypeIndex);
        ref var chunk = ref archetype.Chunks[entityInfo.ChunkIndex];

        writer.WriteNumber("ID", entity.ID);

        if (archetype.HasComponent<SceneRef>())
        {
            ref var sceneRef = ref chunk.GetComponent<SceneRef>(entityInfo.RowIndex);
            if (!options.WriteSubScenes)
            {
                var assetPath = world.Resources.Get<AssetServer>().GetAssets<Scene>().GetPath(sceneRef.Scene);
                if (assetPath.HasValue) writer.WriteString("Scene", assetPath.Value.Path);

                writer.WriteEndObject();
                return;
            }
        }

        {
            writer.WriteStartObject("Components");
            foreach (var componentId in archetype.GetChunkInfo().ComponentIDs)
            {
                if (ignoredComponents.Contains(componentId)) continue;

                var cinfo = Component.GetInfo(componentId);
                writer.WriteStartObject(cinfo.Type.Name);

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

                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        if (archetype.HasComponent<Parent>())
        {
            writer.WriteStartArray("Children");
            ref var parent = ref chunk.GetComponent<Parent>(entityInfo.RowIndex);
            var current = parent.FirstChild;
            while (current != Entity.NULL)
            {
                WriteEntityData(world, current);
                current = world.GetEntityRef(current).Get<Child>().NextSibling;
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void Write<T>(scoped in T value, string? identifier = null) where T : unmanaged
    {
        identifier ??= typeof(T).Name;
        if (BlittableSerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            writer.WriteStartObject(identifier);
            serializer.Serialize(ref this, value, in context);
            writer.WriteEndObject();
        }
        else if (BlittableSerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
        {
            writer.WriteStartObject(identifier);
            defaultSerializer.Serialize(ref this, value, in defaultContext);
            writer.WriteEndObject();
        }
        else
        {
            if (!WriteAsKnownJsonType(value, identifier))
            {
                throw new NotSupportedException($"No blittable writer found for type {typeof(T)}");
            }
        }
    }

    public void Write<T>(T[] values, string? identifier = null) where T : unmanaged
    {
        writer.WriteStartArray(identifier ?? typeof(T).Name);
        foreach (var value in values)
        {
            Write(value, identifier);
        }

        writer.WriteEndArray();
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
        Guard.IsNotNull(identifier, "Missing identifier when writing string value to Scene");
        writer.WriteString(identifier, value);
    }

    public void Serialize<T>(scoped in T value, string? identifier = null) where T : notnull
    {
        writer.WriteStartObject(identifier ?? typeof(T).Name);

        if (SerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            serializer.Serialize(ref this, value, in context);
        }
        else if (SerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
        {
            defaultSerializer.Serialize(ref this, value, in defaultContext);
        }
        else
        {
            if (!WriteAsKnownJsonType(value, identifier!))
            {
                JsonSerializer.Serialize(writer, value);
            }
        }

        writer.WriteEndObject();
    }

    bool WriteAsKnownJsonType<T>(in T value, string identifier)
    {
        if (value is sbyte or short or int)
        {
            writer.WriteNumber(identifier, Unsafe.As<T, int>(ref Unsafe.AsRef(in value)));
            return true;
        }
        else if (value is byte or ushort or uint)
        {
            writer.WriteNumber(identifier, Unsafe.As<T, uint>(ref Unsafe.AsRef(in value)));
            return true;
        }
        else if (value is float)
        {
            writer.WriteNumber(identifier, Unsafe.As<T, float>(ref Unsafe.AsRef(in value)));
            return true;
        }
        else if (value is double)
        {
            writer.WriteNumber(identifier, Unsafe.As<T, double>(ref Unsafe.AsRef(in value)));
            return true;
        }
        else if (value is decimal)
        {
            writer.WriteNumber(identifier, Unsafe.As<T, decimal>(ref Unsafe.AsRef(in value)));
            return true;
        }
        else if (typeof(T).IsEnum)
        {
            // TODO: user underlying type to serialize
            writer.WriteNumber(identifier, Unsafe.As<T, int>(ref Unsafe.AsRef(in value)));
            return true;
        }

        return false;
    }
}
