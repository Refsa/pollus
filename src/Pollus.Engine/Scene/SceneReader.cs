namespace Pollus.Engine;

using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ECS;
using Pollus.Core.Assets;
using Pollus.Core.Serialization;
using Pollus.Assets;
using Utils;

public ref struct SceneReader : IReader, IDisposable
{
    public struct Options()
    {
        public static Options Default => new();

        public int FormatVersion { get; set; } = 1;
        public int TypesVersion { get; set; } = 1;

        public ISceneFileTypeMigration[]? FileTypeMigrations { get; set; }
    }

    // Tracks current component being deserialized
    bool inArray = false;
    JsonElement.ArrayEnumerator currentArray;
    Stack<JsonElement> currentComponent = new();

    Utf8JsonReader reader;
    Scene result;

    Options options;
    WorldSerializationContext context;
    DefaultSerializationContext defaultContext = default;

    public SceneReader()
    {
        result = new Scene()
        {
            Types = [],
            Entities = [],
            Scenes = [],
            Assets = [],
        };
    }

    public SceneReader(Options options) : this()
    {
        this.options = options;
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
        this.reader = new(data);

        while (reader.Read())
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            if (reader.ValueTextEquals("FormatVersion"u8) || reader.ValueTextEquals("formatVersion"u8))
            {
                reader.Read();
                result.FormatVersion = reader.GetInt32();
            }
            else if (reader.ValueTextEquals("TypesVersion"u8) || reader.ValueTextEquals("typesVersion"u8))
            {
                reader.Read();
                result.TypesVersion = reader.GetInt32();
            }
            else if (reader.ValueTextEquals("Types"u8) || reader.ValueTextEquals("types"u8))
            {
                reader.Read();
                ParseTypes(ref reader);
            }
            else if (reader.ValueTextEquals("Entities"u8) || reader.ValueTextEquals("entities"u8))
            {
                reader.Read();
                ParseEntities(ref reader, result.Entities);
            }
        }

        return result;
    }

    void ParseTypes(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject) return;
        var typesVersion = result.TypesVersion;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string key = reader.GetString()!;
                reader.Read();
                string typeName = reader.GetString()!;

                var typeMigration = options.FileTypeMigrations?.FirstOrDefault(migration => migration.FromVersion == typesVersion);

                Type? resolvedType = typeMigration switch
                {
                    { } migration => migration.GetType(key, typeName),
                    null => Type.GetType(typeName),
                };

                if (resolvedType is null)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        resolvedType = assembly.GetType(typeName);
                        if (resolvedType is not null) break;
                    }
                }

                result.Types.Add(key, resolvedType ?? throw new InvalidOperationException($"Type {typeName} not found"));
            }
        }
    }

    void ParseEntities(ref Utf8JsonReader reader, in List<Scene.SceneEntity> entities)
    {
        if (reader.TokenType != JsonTokenType.StartArray) return;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) break;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var entity = ParseEntity(ref reader);
                entities.Add(entity);
            }
        }
    }

    Scene.SceneEntity ParseEntity(ref Utf8JsonReader reader)
    {
        var entity = new Scene.SceneEntity();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            if (reader.ValueTextEquals("ID"u8) || reader.ValueTextEquals("id"u8))
            {
                reader.Read();
                entity.EntityID = reader.GetInt32();
            }
            else if (reader.ValueTextEquals("Name"u8) || reader.ValueTextEquals("name"u8))
            {
                reader.Read();
                entity.Name = reader.GetString();
            }
            else if (reader.ValueTextEquals("Scene"u8) || reader.ValueTextEquals("scene"u8))
            {
                reader.Read();
                var scenePath = reader.GetString();
                if (scenePath != null)
                {
                    var handle = context.AssetServer.GetAssets<Scene>().Initialize(scenePath);
                    entity.Scene = handle;
                    result.Scenes.TryAdd(scenePath, handle);
                }
            }
            else if (reader.ValueTextEquals("Components"u8) || reader.ValueTextEquals("components"u8))
            {
                entity.Components = ParseComponents(ref reader);
            }
            else if (reader.ValueTextEquals("Children"u8) || reader.ValueTextEquals("children"u8))
            {
                reader.Read();
                entity.Children = new();
                ParseEntities(ref reader, entity.Children);
            }
            else
            {
                reader.Read();
                reader.Skip();
            }
        }

        return entity;
    }

    List<Scene.EntityComponent>? ParseComponents(ref Utf8JsonReader reader)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            reader.Skip();
            return null;
        }

        var components = new List<Scene.EntityComponent>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var componentName = reader.GetString()!;
            reader.Read();

            if (result.Types.TryGetValue(componentName, out var type))
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                using var doc = JsonDocument.ParseValue(ref reader);
                var data = DeserializeComponent(doc.RootElement, type);
                var componentInfo = Component.GetInfo(type);
                components.Add(new Scene.EntityComponent
                {
                    ComponentID = componentInfo.ID,
                    Data = data,
                });
            }
            else
            {
                reader.Skip();
            }
        }

        return components;
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