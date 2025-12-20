namespace Pollus.Engine;

using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ECS;
using Pollus.Core.Serialization;
using Pollus.Engine.Serialization;

public ref struct SceneReader : IReader, IDisposable
{
    // Tracks current component being deserialized
    bool inArray = false;
    JsonElement.ArrayEnumerator currentArray;
    Stack<JsonElement> currentComponent = new();

    WorldSerializationContext context;
    DefaultSerializationContext defaultContext = default;

    public SceneReader()
    {
    }

    public void Dispose()
    {
    }

    public void Init(byte[]? data)
    {
        throw new NotSupportedException("SceneReader.Init is not supported, use Parse instead");
    }

    public Scene Parse(in WorldSerializationContext context, in ReadOnlySpan<byte> data)
    {
        this.context = context;

        var document = JsonSerializer.Deserialize<SceneFileData>(data, SceneFileDataJsonSerializerContext.Default.SceneFileData);

        var types = new Dictionary<string, Type>();
        var entities = new List<Scene.SceneEntity>();

        if (document.Types is not null)
        {
            foreach (var type in document.Types)
            {
                var resolvedType = System.Type.GetType(type.Value);
                if (resolvedType is null)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        resolvedType = assembly.GetType(type.Value);
                        if (resolvedType is not null) break;
                    }
                }

                types.Add(type.Key, resolvedType ?? throw new Exception($"Type {type.Value} not found"));
            }
        }

        if (document.Entities is not null)
        {
            foreach (var entity in document.Entities)
            {
                ParseEntity(entity, types, entities);
            }
        }

        return new Scene()
        {
            Types = types,
            Entities = entities,
        };
    }

    void ParseEntity(in SceneFileData.EntityData entity, in IReadOnlyDictionary<string, Type> types, in List<Scene.SceneEntity> entities)
    {
        var sceneEntity = new Scene.SceneEntity()
        {
            Name = entity.Name,
            EntityID = entity.ID,
        };

        if (entity.Components is not null)
        {
            sceneEntity.Components = [];
            foreach (var component in entity.Components)
            {
                var type = types[component.Key];
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);

                var data = DeserializeComponent(component.Value, type);
                var componentInfo = Component.GetInfo(type);
                sceneEntity.Components.Add(new Scene.EntityComponent()
                {
                    ComponentID = componentInfo.ID,
                    Data = data,
                });
            }
        }

        if (entity.Children is not null)
        {
            sceneEntity.Children = [];
            foreach (var child in entity.Children)
            {
                ParseEntity(child, types, sceneEntity.Children);
            }
        }

        entities.Add(sceneEntity);
    }

    byte[] DeserializeComponent(in JsonElement component, in Type type)
    {
        try
        {
            currentComponent.Push(component);

            if (BlittableSerializerLookup<WorldSerializationContext>.GetSerializer(type) is { } blittableSerializer)
            {
                return blittableSerializer.DeserializeBytes(ref this, in context);
            }

            if (BlittableSerializerLookup<DefaultSerializationContext>.GetSerializer(type) is { } defaultSerializer)
            {
                return defaultSerializer.DeserializeBytes(ref this, defaultContext);
            }
        }
        finally
        {
            currentComponent.Clear();
        }

        throw new InvalidOperationException($"No serializer found for type {type.FullName}");
    }

    T ReadJsonObject<T>(JsonElement jsonElement)
        where T : unmanaged
    {
        try
        {
            currentComponent.Push(jsonElement);

            if (BlittableSerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } blittableSerializer)
            {
                return blittableSerializer.Deserialize(ref this, context);
            }

            if (BlittableSerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
            {
                return defaultSerializer.Deserialize(ref this, defaultContext);
            }
        }
        finally
        {
            currentComponent.Pop();
        }

        throw new Exception($"No blittable serializer found for {typeof(T).AssemblyQualifiedName}");
    }

    public string? ReadString(string? identifier = null)
    {
        if (currentComponent.Peek().ValueKind == JsonValueKind.String)
        {
            return currentComponent.Peek().GetString();
        }
        else if (identifier != null && currentComponent.Peek().TryGetProperty(identifier, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.String) return prop.GetString();
        }

        return null;
    }

    public T Read<T>(string? identifier = null) where T : unmanaged
    {
        var prop = (identifier, inArray) switch
        {
            (_, true) => currentComponent.Peek(),
            (null, _) => currentComponent.Peek(),
            (not null, _) => currentComponent.Peek().TryGetProperty(identifier, out var p) ? p : (JsonElement?)null,
        };

        if (prop is null)
        {
            return default;
        }

        if (prop.Value.ValueKind is JsonValueKind.Array || inArray)
        {
            if (inArray is false)
            {
                inArray = true;
                currentArray = prop.Value.EnumerateArray();
                currentArray.MoveNext();
                return ReadJsonObject<T>(prop.Value);
            }

            prop = currentArray.Current;
            if (!currentArray.MoveNext())
            {
                inArray = false;
            }
        }

        if (typeof(T).IsEnum && prop.Value.ValueKind is JsonValueKind.String)
        {
            var value = prop.Value.GetString();
            if (string.IsNullOrEmpty(value)) return default;
            return Enum.Parse<T>(value);
        }

        return prop.Value.ValueKind switch
        {
            JsonValueKind.Object or JsonValueKind.String => ReadJsonObject<T>(prop.Value),
            JsonValueKind.Number => typeof(T) switch
            {
                var v when v == typeof(float) => Unsafe.BitCast<float, T>((float)prop.Value.GetDouble()),
                var v when v == typeof(double) => Unsafe.BitCast<double, T>((double)prop.Value.GetDouble()),
                var v when v == typeof(sbyte) => Unsafe.BitCast<sbyte, T>(prop.Value.GetSByte()),
                var v when v == typeof(short) => Unsafe.BitCast<short, T>(prop.Value.GetInt16()),
                var v when v == typeof(int) => Unsafe.BitCast<int, T>(prop.Value.GetInt32()),
                var v when v == typeof(long) => Unsafe.BitCast<long, T>(prop.Value.GetInt64()),
                var v when v == typeof(byte) => Unsafe.BitCast<byte, T>(prop.Value.GetByte()),
                var v when v == typeof(ushort) => Unsafe.BitCast<ushort, T>(prop.Value.GetUInt16()),
                var v when v == typeof(uint) => Unsafe.BitCast<uint, T>(prop.Value.GetUInt32()),
                var v when v == typeof(ulong) => Unsafe.BitCast<ulong, T>(prop.Value.GetUInt64()),
                _ => throw new NotSupportedException(),
            },
            JsonValueKind.True => Unsafe.BitCast<bool, T>(prop.Value.GetBoolean()),
            JsonValueKind.False => Unsafe.BitCast<bool, T>(prop.Value.GetBoolean()),
            JsonValueKind.Undefined or JsonValueKind.Null => default,
            _ => throw new ArgumentOutOfRangeException(nameof(prop.Value.ValueKind)),
        };
    }

    public T[] ReadArray<T>(string? identifier = null) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public T Deserialize<T>(string? identifier = null) where T : notnull
    {
        var prop = identifier switch
        {
            null => currentComponent.Peek(),
            not null => currentComponent.Peek().GetProperty(identifier),
        };

        try
        {
            currentComponent.Push(prop);

            if (SerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } blittableSerializer)
            {
                return blittableSerializer.Deserialize(ref this, in context);
            }

            if (SerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
            {
                return defaultSerializer.Deserialize(ref this, defaultContext);
            }
        }
        finally
        {
            currentComponent.Pop();
        }

        throw new InvalidOperationException($"No serializer found for type {typeof(T).AssemblyQualifiedName}");
    }
}