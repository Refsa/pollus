namespace Pollus.Engine;

using Pollus.ECS;
using Pollus.Debugging;
using Pollus.Utils;
using Pollus.Engine.Serialization;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Pollus.Core.Serialization;

public ref struct SceneReader : IReader, IDisposable
{
    ReadOnlySpan<byte> data;
    int cursor;
    int length;
    Dictionary<string, Type> types;
    WorldSerializationContext context;

    public void Init(byte[]? data)
    {
        this.data = data switch
        {
            null => ReadOnlySpan<byte>.Empty,
            { } d => d.AsSpan()
        };
        this.cursor = 0;
        this.length = this.data.Length;
        this.types = Pool<Dictionary<string, Type>>.Shared.Rent();
    }

    public void Dispose()
    {
        if (types != null)
        {
            types.Clear();
            Pool<Dictionary<string, Type>>.Shared.Return(types);
            types = null!;
        }
    }

    int PeekIndent()
    {
        int p = cursor;
        if (p > 0 && data[p - 1] != '\n')
        {
            while (p < length && data[p] != '\n') p++;
            if (p >= length) return -1;
            p++;
        }

        while (p < length)
        {
            int start = p;
            while (p < length && data[p] == ' ') p++;

            if (p < length && ((char)data[p] is '\n' or '\r' or '#'))
            {
                while (p < length && data[p] != '\n') p++;
                if (p >= length) return -1;
                p++;
                continue;
            }
            if (p >= length) return -1;

            return p - start;
        }

        return -1;
    }

    void AdvanceToNextLine()
    {
        if (cursor > 0 && data[cursor - 1] != '\n')
        {
            while (cursor < length && data[cursor] != '\n') cursor++;
            if (cursor >= length) return;
            cursor++;
        }

        while (cursor < length)
        {
            int p = cursor;
            while (p < length && data[p] == ' ') p++;
            if (p < length && ((char)data[p] is '\n' or '\r' or '#'))
            {
                cursor = p;
                while (cursor < length && data[cursor] != '\n') cursor++;
                if (cursor < length) cursor++;
                continue;
            }

            return;
        }
    }

    void SkipIndent()
    {
        while (cursor < length && data[cursor] == ' ') cursor++;
    }

    void SkipWhitespace()
    {
        while (cursor < length && ((char)data[cursor] is ' ' or '\n' or '\r')) cursor++;
    }

    void SkipSpaces()
    {
        while (cursor < length && ((char)data[cursor] is ' ' or '\r')) cursor++;
    }

    void SkipKey()
    {
        SkipStructural();
        int p = cursor;
        while (p < length && IsIdentifierChar((char)data[p])) p++;
        while (p < length && data[p] == ' ') p++;
        if (p < length && data[p] == ':')
        {
            cursor = p + 1;
        }
    }

    void SkipStructural()
    {
        while (cursor < length)
        {
            char c = (char)data[cursor];
            if (c is ' ' or '\n' or '\r' or '{' or ',')
            {
                cursor++;
            }
            else
            {
                break;
            }
        }
    }

    static bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-';
    }

    void ParseTypes(List<Scene.Type> sceneTypes, int parentIndent)
    {
        while (cursor < length)
        {
            int indent = PeekIndent();
            if (indent <= parentIndent) break;

            AdvanceToNextLine();
            SkipIndent();

            string alias = ReadIdentifier();
            if (string.IsNullOrEmpty(alias)) continue;

            if (Match(':'))
            {
                SkipSpaces();
                string typeName = ReadString();

                if (ResolveType(typeName) is { } t)
                {
                    types[alias] = t;
                    sceneTypes.Add(new Scene.Type
                    {
                        ID = sceneTypes.Count,
                        Name = alias,
                        AssemblyQualifiedName = t.AssemblyQualifiedName ?? typeName
                    });
                }
            }
        }
    }

    Type? ResolveType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName)) return null;
        Type? t = Type.GetType(typeName);
        if (t == null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(typeName);
                if (t != null) break;
            }
        }

        return t;
    }

    void ParseEntities(List<Scene.Entity> entities, int parentIndent)
    {
        while (cursor < length)
        {
            int indent = PeekIndent();
            if (indent <= parentIndent) break;

            AdvanceToNextLine();
            SkipIndent();

            string entityName = ReadIdentifier();
            if (string.IsNullOrEmpty(entityName)) continue;
            if (!Match(':')) continue;

            var entity = new Scene.Entity { Name = entityName };
            var components = new List<Scene.Component>();
            var children = new List<Scene.Entity>();

            ParseEntityBody(ref entity, components, children, indent);

            entity.Components = components;
            entity.Children = children;
            entities.Add(entity);
        }
    }

    void ParseEntityBody(ref Scene.Entity entity, List<Scene.Component> components, List<Scene.Entity> children, int entityIndent)
    {
        while (cursor < length)
        {
            int propIndent = PeekIndent();
            if (propIndent <= entityIndent) break;

            AdvanceToNextLine();
            SkipIndent();

            string prop = ReadIdentifier();
            if (string.IsNullOrEmpty(prop)) continue;
            if (!Match(':')) continue;
            SkipSpaces();

            if (prop == "id")
            {
                string idVal = ReadString();
                if (int.TryParse(idVal, out int id)) entity.EntityID = id;
            }
            else if (prop == "components")
            {
                ParseComponents(components, propIndent);
            }
            else if (prop == "children")
            {
                ParseChildren(children, propIndent);
            }
            else
            {
                SkipValue();
            }
        }
    }

    void ParseComponents(List<Scene.Component> components, int parentIndent)
    {
        while (cursor < length)
        {
            int compIndent = PeekIndent();
            if (compIndent <= parentIndent) break;

            AdvanceToNextLine();
            SkipIndent();

            string typeAlias = ReadIdentifier();
            if (string.IsNullOrEmpty(typeAlias)) continue;
            if (!Match(':')) continue;
            SkipSpaces();

            if (types.TryGetValue(typeAlias, out Type? type))
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                var cid = Component.GetInfo(type).ID;

                components.Add(new Scene.Component
                {
                    TypeID = -1,
                    ComponentID = cid.ID,
                    Data = DeserializeComponent(type),
                });
            }
            else
            {
                SkipValue();
            }
        }
    }

    void ParseChildren(List<Scene.Entity> children, int parentIndent)
    {
        SkipSpaces();
        if (cursor < length && data[cursor] == '[')
        {
            cursor++;
            while (cursor < length && data[cursor] != ']') cursor++;
            if (cursor < length) cursor++;
            return;
        }

        while (cursor < length)
        {
            int childIndent = PeekIndent();
            if (childIndent <= parentIndent) break;

            AdvanceToNextLine();
            SkipIndent();

            string childName = ReadIdentifier();
            if (string.IsNullOrEmpty(childName)) continue;
            if (!Match(':')) continue;

            var child = new Scene.Entity { Name = childName };
            var childComps = new List<Scene.Component>();
            var childKids = new List<Scene.Entity>();

            ParseEntityBody(ref child, childComps, childKids, childIndent);

            child.Components = childComps;
            child.Children = childKids;
            children.Add(child);
        }
    }

    byte[] DeserializeComponent(Type type)
    {
        Guard.IsNotNull(context.AssetServer, "AssetServer was null");

        if (BlittableSerializerLookup<WorldSerializationContext>.GetSerializer(type) is { } serializer)
        {
            return serializer.DeserializeBytes(ref this, in context);
        }
        else if (BlittableSerializerLookup<DefaultSerializationContext>.GetSerializer(type) is { } defaultSerializer)
        {
            return defaultSerializer.DeserializeBytes(ref this, new());
        }

        throw new InvalidOperationException($"No serializer found for type {type.AssemblyQualifiedName}");
    }

    T DeserializeBlittable<T>() where T : unmanaged
    {
        Guard.IsNotNull(context.AssetServer, "AssetServer was null");

        if (SerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            return serializer.Deserialize(ref this, in context);
        }
        else if (SerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
        {
            return defaultSerializer.Deserialize(ref this, new());
        }

        throw new InvalidOperationException($"No serializer found for type {typeof(T).AssemblyQualifiedName}");
    }

    public Scene Parse(in WorldSerializationContext context, ReadOnlySpan<byte> data)
    {
        this.data = data;
        this.cursor = 0;
        this.length = this.data.Length;
        this.types = Pool<Dictionary<string, Type>>.Shared.Rent();
        this.context = context;

        var sceneTypes = new List<Scene.Type>();
        var entities = new List<Scene.Entity>();

        while (cursor < length)
        {
            int indent = PeekIndent();
            if (indent == -1) break;

            AdvanceToNextLine();
            SkipIndent();

            string key = ReadIdentifier();
            if (string.IsNullOrEmpty(key)) break;

            if (!Match(':')) break;
            SkipSpaces();

            if (key == "types") ParseTypes(sceneTypes, indent);
            else if (key == "entities") ParseEntities(entities, indent);
            else SkipValue();
        }

        return new Scene
        {
            Types = sceneTypes,
            Entities = entities,
            ComponentInfos = []
        };
    }

    public bool Match(char c)
    {
        if (cursor < length && data[cursor] == c)
        {
            cursor++;
            return true;
        }

        return false;
    }

    public string ReadIdentifier()
    {
        int start = cursor;
        while (cursor < length && IsIdentifierChar((char)data[cursor])) cursor++;
        return Encoding.UTF8.GetString(data.Slice(start, cursor - start));
    }

    public string ReadString()
    {
        // Attempt to skip key on current line
        int p = cursor;
        while (p < length && data[p] == ' ') p++;

        if (p < length && IsIdentifierChar((char)data[p]))
        {
            int scan = p;
            while (scan < length && IsIdentifierChar((char)data[scan])) scan++;
            while (scan < length && data[scan] == ' ') scan++;

            if (scan < length && data[scan] == ':')
            {
                cursor = scan + 1;
            }
        }

        SkipSpaces();
        if (cursor >= length) return "";

        if (data[cursor] == '"')
        {
            cursor++;
            int start = cursor;
            while (cursor < length && data[cursor] != '"') cursor++;
            string s = Encoding.UTF8.GetString(data.Slice(start, cursor - start));
            if (cursor < length) cursor++;
            return s;
        }
        else
        {
            int start = cursor;
            while (cursor < length && data[cursor] != '\n' && data[cursor] != ',' && data[cursor] != '}' && data[cursor] != ']') cursor++;

            int end = cursor;
            while (end > start && (data[end - 1] == ' ' || data[end - 1] == '\r')) end--;

            return Encoding.UTF8.GetString(data.Slice(start, end - start));
        }
    }

    public void SkipValue()
    {
        int startIndent = PeekIndent();
        AdvanceToNextLine();
        while (cursor < length)
        {
            int ind = PeekIndent();
            if (ind != -1 && ind <= startIndent) break;
            AdvanceToNextLine();
        }
    }

    public T Deserialize<T>() where T : notnull
    {
        if (SerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            return serializer.Deserialize(ref this, in context);
        }
        else if (SerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
        {
            return defaultSerializer.Deserialize(ref this, new());
        }

        throw new InvalidOperationException($"No serializer found for type {typeof(T).AssemblyQualifiedName}");
    }

    public T Read<T>() where T : unmanaged
    {
        Type t = typeof(T);
        if (t.IsPrimitive || t.IsEnum)
        {
            SkipKey();

            string val = ReadString();
            if (string.IsNullOrEmpty(val)) return default;

            if (t == typeof(int)) return Unsafe.BitCast<int, T>(int.Parse(val));
            if (t == typeof(float)) return Unsafe.BitCast<float, T>(float.Parse(val, CultureInfo.InvariantCulture));
            if (t == typeof(double)) return Unsafe.BitCast<double, T>(double.Parse(val, CultureInfo.InvariantCulture));
            if (t == typeof(bool)) return Unsafe.BitCast<bool, T>(bool.Parse(val));
            if (t.IsEnum) return (T)Enum.Parse(t, val);
            return default;
        }

        SkipKey();
        return DeserializeBlittable<T>();
    }

    public T[] ReadArray<T>() where T : unmanaged
    {
        throw new NotImplementedException();
    }
}
