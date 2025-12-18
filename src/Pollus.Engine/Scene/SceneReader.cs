namespace Pollus.Engine;

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
        throw new NotSupportedException();
    }

    public void Dispose()
    {
        types.Clear();
        Pool<Dictionary<string, Type>>.Shared.Return(types);
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
            SkipAllWhitespace();
            if (cursor >= length) break;

            string key = ReadIdentifier();
            if (string.IsNullOrEmpty(key)) break;

            if (!Match(':')) break;
            SkipSpaces();

            if (key == "types")
            {
                ParseTypes(sceneTypes);
            }
            else if (key == "entities")
            {
                ParseEntities(entities);
            }
            else
            {
                SkipValue();
            }
        }

        return new Scene
        {
            Types = sceneTypes.ToArray(),
            Entities = entities.ToArray()
        };
    }

    void ParseTypes(List<Scene.Type> sceneTypes)
    {
        int indent = GetCurrentIndent();
        while (cursor < length)
        {
            int nextIndent = PeekIndent();
            if (nextIndent <= indent) break;

            AdvanceToNextLine();
            SkipIndent();

            string alias = ReadIdentifier();
            if (string.IsNullOrEmpty(alias)) continue;

            if (Match(':'))
            {
                SkipSpaces();
                string typeName = ReadValueString();

                Type? t = Type.GetType(typeName);
                if (t == null)
                {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        t = asm.GetType(typeName);
                        if (t != null) break;
                    }
                }

                if (t != null)
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

    void ParseEntities(List<Scene.Entity> entities)
    {
        int parentIndent = GetCurrentIndent();

        while (cursor < length)
        {
            int nextIndent = PeekIndent();
            if (nextIndent <= parentIndent) break;

            AdvanceToNextLine();
            SkipIndent();

            string entityName = ReadIdentifier();
            if (string.IsNullOrEmpty(entityName)) continue;

            if (!Match(':')) continue;

            var entity = new Scene.Entity { Name = entityName };
            var components = new List<Scene.Component>();
            var children = new List<Scene.Entity>();

            ParseEntityBody(entityName, ref entity, components, children, nextIndent);

            entity.Components = components.ToArray();
            entity.Children = children.ToArray();
            entities.Add(entity);
        }
    }

    void ParseEntityBody(string name, ref Scene.Entity entity, List<Scene.Component> components, List<Scene.Entity> children, int entityIndent)
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
                string idVal = ReadValueString();
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
                components.Add(new Scene.Component
                {
                    TypeID = -1,
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

            ParseEntityBody(childName, ref child, childComps, childKids, childIndent);

            child.Components = childComps.ToArray();
            child.Children = childKids.ToArray();
            children.Add(child);
        }
    }

    byte[] DeserializeComponent(Type type)
    {
        if (BlittableSerializerLookup<WorldSerializationContext>.GetSerializer(type) is { } serializer)
        {
            return serializer.DeserializeBytes(ref this, in context);
        }
        else if (BlittableSerializerLookup<DefaultSerializationContext>.GetSerializer(type) is { } defaultSerializer)
        {
            return defaultSerializer.DeserializeBytes(ref this, new());
        }

        throw new InvalidOperationException($"No serializer found for type {type.Name}");
    }

    T Deserialize<T>()
        where T : unmanaged
    {
        if (SerializerLookup<WorldSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            return serializer.Deserialize(ref this, in context);
        }
        else if (SerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } defaultSerializer)
        {
            return defaultSerializer.Deserialize(ref this, new());
        }

        throw new InvalidOperationException($"No serializer found for type {typeof(T).Name}");
    }

    int GetCurrentIndent()
    {
        return 0;
    }

    int PeekIndent()
    {
        int p = cursor;
        while (p < length && data[p] != '\n') p++;
        if (p >= length) return -1;
        p++;

        int spaces = 0;
        while (p < length && data[p] == ' ')
        {
            spaces++;
            p++;
        }

        if (p < length && (data[p] == '\n' || data[p] == '#')) return 1000;
        return spaces;
    }

    void AdvanceToNextLine()
    {
        while (cursor < length && data[cursor] != '\n') cursor++;
        if (cursor < length) cursor++;
    }

    void SkipIndent()
    {
        while (cursor < length && data[cursor] == ' ') cursor++;
    }

    void SkipAllWhitespace()
    {
        while (cursor < length && (data[cursor] == ' ' || data[cursor] == '\n' || data[cursor] == '\r')) cursor++;
    }

    void SkipSpaces()
    {
        while (cursor < length && (data[cursor] == ' ' || data[cursor] == '\r')) cursor++;
    }

    bool Match(char c)
    {
        if (cursor < length && data[cursor] == c)
        {
            cursor++;
            return true;
        }

        return false;
    }

    string ReadIdentifier()
    {
        int start = cursor;
        while (cursor < length && IsIdentifierChar((char)data[cursor])) cursor++;
        return Encoding.UTF8.GetString(data.Slice(start, cursor - start));
    }

    bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-';
    }

    string ReadValueString()
    {
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

    void SkipValue()
    {
        int startIndent = PeekIndent();
        AdvanceToNextLine();
        while (cursor < length)
        {
            int ind = PeekIndent();
            if (ind <= startIndent && ind != 1000) break;
            AdvanceToNextLine();
        }
    }

    public string ReadString()
    {
        SkipKey();
        return ReadValueString();
    }

    public T Read<T>() where T : unmanaged
    {
        Type t = typeof(T);
        if (t.IsPrimitive || t.IsEnum)
        {
            SkipKey();
            SkipStructural();
            string val = ReadValueString();
            if (t == typeof(int)) return Unsafe.BitCast<int, T>(int.Parse(val));
            if (t == typeof(float)) return Unsafe.BitCast<float, T>(float.Parse(val, CultureInfo.InvariantCulture));
            if (t == typeof(double)) return Unsafe.BitCast<double, T>(double.Parse(val, CultureInfo.InvariantCulture));
            if (t == typeof(bool)) return Unsafe.BitCast<bool, T>(bool.Parse(val));
            if (t.IsEnum) return (T)Enum.Parse(t, val);
            return default;
        }

        SkipKey();
        return Deserialize<T>();
    }

    public T[] ReadArray<T>() where T : unmanaged
    {
        throw new NotImplementedException();
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
            if (c == ' ' || c == '\n' || c == '\r' || c == '{' || c == '}' || c == ',')
            {
                cursor++;
            }
            else
            {
                break;
            }
        }
    }
}
