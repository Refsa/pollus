namespace Pollus.Assets;

using System.Runtime.CompilerServices;
using Pollus.Core.Assets;
using Pollus.Core.Serialization;
using Pollus.Utils;

public struct WorldSerializationContext()
{
    public required AssetServer AssetServer { get; set; }
    public HashSet<Handle> Dependencies { get; set; } = [];
}

public class HandleSerializer<T> : IBlittableSerializer<Handle<T>, WorldSerializationContext>
    where T : IAsset
{
    public Handle<T> Deserialize<TReader>(ref TReader reader, in WorldSerializationContext context)
        where TReader : IReader, allows ref struct
    {
        var path = reader.ReadString("$path");

        Handle handle;
        if (string.IsNullOrEmpty(path))
        {
            var asset = reader.Deserialize<T>();
            handle = context.AssetServer.Assets.AddAsset(asset);
        }
        else
        {
            handle = context.AssetServer.LoadAsync<T>(path);
        }

        context.Dependencies.Add(handle);
        return handle;
    }

    public void Serialize<TWriter>(ref TWriter writer, in Handle<T> value, in WorldSerializationContext context)
        where TWriter : IWriter, allows ref struct
    {
        if (value.IsNull()) return;

        var info = context.AssetServer.Assets.GetInfo(value);
        if (info?.Path is { } path)
        {
            writer.Write(path.Path, "$path");
        }
        else
        {
            var asset = context.AssetServer.Assets.GetAsset(value);
            if (asset is not null)
            {
                writer.Serialize(asset);
            }
        }
    }
}

public class UntypedHandleSerializer : IBlittableSerializer<Handle, WorldSerializationContext>
{
    static Type ResolveType(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type is not null) return type;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type is not null) return type;
        }

        throw new InvalidOperationException($"Could not resolve type '{typeName}' for untyped Handle");
    }

    public Handle Deserialize<TReader>(ref TReader reader, in WorldSerializationContext context)
        where TReader : IReader, allows ref struct
    {
        var typeName = reader.ReadString("$type")
                       ?? throw new InvalidOperationException("Untyped Handle requires a '$type' property");

        var resolvedType = ResolveType(typeName);

        if (!typeof(IAsset).IsAssignableFrom(resolvedType))
            throw new InvalidOperationException($"Type '{typeName}' does not implement IAsset");

        var typeId = TypeLookup.ID(resolvedType)
                     ?? throw new InvalidOperationException($"Asset storage not initialized for type '{resolvedType.FullName}'. Call InitAssets<{resolvedType.Name}>() before deserializing.");
        var path = reader.ReadString("$path");

        Handle handle;
        if (!string.IsNullOrEmpty(path))
        {
            handle = context.AssetServer.Load(path);
            if (handle.IsNull())
                throw new InvalidOperationException($"Failed to load asset from path '{path}' for type '{resolvedType.FullName}'");
        }
        else
        {
            IAsset asset;
            if (SerializerLookup<WorldSerializationContext>.GetSerializer(resolvedType) is { } worldSerializer)
            {
                asset = worldSerializer.DeserializeBoxed(ref reader, in context) as IAsset
                        ?? throw new InvalidOperationException($"Deserialized object for type '{resolvedType.FullName}' is not an IAsset");
            }
            else if (SerializerLookup<DefaultSerializationContext>.GetSerializer(resolvedType) is { } defaultSerializer)
            {
                DefaultSerializationContext defaultCtx = default;
                asset = defaultSerializer.DeserializeBoxed(ref reader, in defaultCtx) as IAsset
                        ?? throw new InvalidOperationException($"Deserialized object for type '{resolvedType.FullName}' is not an IAsset");
            }
            else
            {
                throw new InvalidOperationException($"No serializer found for asset type '{resolvedType.FullName}'");
            }

            handle = context.AssetServer.Assets.AddAsset(asset, typeId);
        }

        context.Dependencies.Add(handle);
        return handle;
    }

    public void Serialize<TWriter>(ref TWriter writer, in Handle value, in WorldSerializationContext context)
        where TWriter : IWriter, allows ref struct
    {
        if (value.IsNull()) return;

        var runtimeType = TypeLookup.GetType(value.Type)
                          ?? throw new InvalidOperationException($"Could not resolve runtime type for TypeID {value.Type}");

        writer.Write(runtimeType.AssemblyQualifiedName!, "$type");

        var info = context.AssetServer.Assets.GetInfo(value);
        if (info?.Path is { } path)
        {
            writer.Write(path.Path, "$path");
        }
        else
        {
            var asset = context.AssetServer.Assets.GetAsset(value);
            if (asset is not null)
            {
                if (SerializerLookup<WorldSerializationContext>.GetSerializer(runtimeType) is { } worldSerializer)
                {
                    worldSerializer.SerializeBoxed(ref writer, asset, in context);
                }
                else if (SerializerLookup<DefaultSerializationContext>.GetSerializer(runtimeType) is { } defaultSerializer)
                {
                    DefaultSerializationContext defaultCtx = default;
                    defaultSerializer.SerializeBoxed(ref writer, asset, in defaultCtx);
                }
            }
        }
    }

#pragma warning disable CA2255
    [ModuleInitializer]
    internal static void UntypedHandleSerializer_ModuleInitializer()
    {
        BlittableSerializerLookup<WorldSerializationContext>.RegisterSerializer(new UntypedHandleSerializer());
    }
#pragma warning restore CA2255
}
