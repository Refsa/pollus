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
using Pollus.Graphics.Rendering;

public ref struct SceneReader : IReader, IDisposable
{
    enum NodeKind
    {
        Scalar,
        Mapping,
        Sequence,
    }

    struct Node
    {
        public NodeKind Kind;
        public string? Scalar;
        public List<KeyValuePair<string, Node>>? Map;
        public List<Node>? Sequence;
    }

    struct ValueContext
    {
        public Node Node;
        public int Index;
        public bool UseNode;
    }

    ReadOnlySpan<byte> data;
    int cursor;
    int length;
    Dictionary<string, Type> types;
    WorldSerializationContext context;
    List<ValueContext> contexts;

    public void Init(byte[]? data)
    {
        this.data = data switch
        {
            null => ReadOnlySpan<byte>.Empty,
            { } d => d.AsSpan()
        };
        cursor = 0;
        length = this.data.Length;
        types = Pool<Dictionary<string, Type>>.Shared.Rent();
        types.Clear();
        contexts = contexts ?? new List<ValueContext>(8);
        contexts.Clear();
    }

    public Scene Parse(in WorldSerializationContext context, in ReadOnlySpan<byte> data)
    {
        this.data = data;
        cursor = 0;
        length = this.data.Length;
        types = Pool<Dictionary<string, Type>>.Shared.Rent();
        types.Clear();
        contexts = contexts ?? new List<ValueContext>(8);
        contexts.Clear();
        this.context = context;

        var root = ParseDocument();
        return BuildScene(root);
    }

    Scene BuildScene(Node root)
    {
        var sceneTypes = new List<Scene.Type>();
        var sceneEntities = new List<Scene.Entity>();
        var componentInfos = new List<Scene.ComponentInfo>();

        if (root.Map != null)
        {
            for (int i = 0; i < root.Map.Count; i++)
            {
                var entry = root.Map[i];
                if (entry.Key == "types" && entry.Value.Kind == NodeKind.Mapping)
                {
                    var typeMap = entry.Value.Map;
                    if (typeMap != null)
                    {
                        for (int t = 0; t < typeMap.Count; t++)
                        {
                            var typeEntry = typeMap[t];
                            var aqn = typeEntry.Value.Scalar ?? string.Empty;
                            var type = Type.GetType(aqn);
                            if (type != null)
                            {
                                types[typeEntry.Key] = type;
                            }
                            sceneTypes.Add(new Scene.Type
                            {
                                ID = sceneTypes.Count,
                                Name = typeEntry.Key,
                                AssemblyQualifiedName = aqn,
                            });
                        }
                    }
                }
            }

            for (int i = 0; i < root.Map.Count; i++)
            {
                var entry = root.Map[i];
                if (entry.Key == "entities" && entry.Value.Kind == NodeKind.Mapping)
                {
                    var entityMap = entry.Value.Map;
                    if (entityMap != null)
                    {
                        for (int e = 0; e < entityMap.Count; e++)
                        {
                            var entity = BuildEntity(entityMap[e].Key, entityMap[e].Value, sceneTypes);
                            sceneEntities.Add(entity);
                        }
                    }
                }
            }
        }

        return new Scene
        {
            Types = sceneTypes,
            Entities = sceneEntities,
            ComponentInfos = componentInfos,
        };
    }

    Scene.Entity BuildEntity(string name, Node node, List<Scene.Type> sceneTypes)
    {
        var entity = new Scene.Entity
        {
            Name = name,
            EntityID = 0,
            Components = new List<Scene.Component>(),
            Children = new List<Scene.Entity>(),
        };

        if (node.Map == null)
        {
            return entity;
        }

        for (int i = 0; i < node.Map.Count; i++)
        {
            var entry = node.Map[i];
            if (entry is { Key: "id", Value.Kind: NodeKind.Scalar })
            {
                entity.EntityID = ParseInt(entry.Value.Scalar);
            }
            else if (entry is { Key: "components", Value.Kind: NodeKind.Mapping })
            {
                var compMap = entry.Value.Map;
                if (compMap != null)
                {
                    for (int c = 0; c < compMap.Count; c++)
                    {
                        var compEntry = compMap[c];
                        var compType = ResolveType(compEntry.Key);
                        if (compType == null)
                        {
                            continue;
                        }

                        entity.Components.Add(new Scene.Component
                        {
                            TypeID = EnsureType(compType, compEntry.Key, sceneTypes),
                            ComponentID = Component.GetInfo(compType).ID,
                            Data = DeserializeComponent(compType, compEntry.Value),
                        });
                    }
                }
            }
            else if (entry is { Key: "children", Value.Kind: NodeKind.Mapping })
            {
                var childMap = entry.Value.Map;
                if (childMap != null)
                {
                    for (int c = 0; c < childMap.Count; c++)
                    {
                        var childEntry = childMap[c];
                        var child = BuildEntity(childEntry.Key, childEntry.Value, sceneTypes);
                        entity.Children.Add(child);
                    }
                }
            }
        }

        return entity;
    }

    Type? ResolveType(string name)
    {
        if (types.TryGetValue(name, out var cached))
        {
            return cached;
        }

        var type = Type.GetType(name);

        if (type != null)
        {
            types[name] = type;
        }

        return type;
    }

    int EnsureType(Type type, string name, List<Scene.Type> sceneTypes)
    {
        for (int i = 0; i < sceneTypes.Count; i++)
        {
            if (sceneTypes[i].Name == name)
            {
                return sceneTypes[i].ID;
            }
        }

        var id = sceneTypes.Count;
        sceneTypes.Add(new Scene.Type
        {
            ID = id,
            Name = name,
            AssemblyQualifiedName = type.AssemblyQualifiedName ?? name,
        });
        types[name] = type;
        return id;
    }

    byte[] DeserializeComponent(Type type, Node node)
    {
        var blittableSerializer = BlittableSerializerLookup<WorldSerializationContext>.GetSerializer(type);
        if (blittableSerializer != null)
        {
            PushContext(node);
            var data = blittableSerializer.DeserializeBytes(ref this, in context);
            PopContext();
            return data;
        }

        var serializer = SerializerLookup<WorldSerializationContext>.GetSerializer(type);
        if (serializer != null)
        {
            PushContext(node);
            var boxed = serializer.DeserializeBoxed(ref this, in context);
            PopContext();
            if (boxed != null)
            {
                return SerializeObject(boxed);
            }
        }

        var defaultSerializer = SerializerLookup<DefaultSerializationContext>.GetSerializer(type);
        if (defaultSerializer != null)
        {
            PushContext(node);
            var boxed = defaultSerializer.DeserializeBoxed(ref this, new DefaultSerializationContext());
            PopContext();
            if (boxed != null)
            {
                return SerializeObject(boxed);
            }
        }

        return Array.Empty<byte>();
    }

    byte[] SerializeObject(object value)
    {
        var type = value.GetType();
        if (type.IsValueType)
        {
            var size = Unsafe.SizeOf<object>();
            size = Unsafe.SizeOf<object>(); // placeholder to satisfy compiler
        }

        if (value is byte[] bytes)
        {
            return bytes;
        }

        return Array.Empty<byte>();
    }

    void PushContext(Node node, bool useNode = false)
    {
        contexts.Add(new ValueContext { Node = node, Index = 0, UseNode = useNode });
    }

    void PopContext()
    {
        if (contexts.Count > 0)
        {
            contexts.RemoveAt(contexts.Count - 1);
        }
    }

    Node? TakeContextNode()
    {
        var count = contexts.Count;
        if (count == 0)
        {
            return null;
        }

        var ctx = contexts[count - 1];
        if (ctx.UseNode && ctx.Index == 0)
        {
            ctx.UseNode = false;
            contexts[count - 1] = ctx;
            return ctx.Node;
        }

        return null;
    }

    Node? PeekValue()
    {
        var count = contexts.Count;
        while (count > 0)
        {
            var ctx = contexts[count - 1];
            if (ctx.Node.Kind == NodeKind.Mapping)
            {
                var map = ctx.Node.Map;
                if (map != null && ctx.Index < map.Count)
                {
                    return map[ctx.Index].Value;
                }
            }
            else if (ctx.Node.Kind == NodeKind.Sequence)
            {
                var seq = ctx.Node.Sequence;
                if (seq != null && ctx.Index < seq.Count)
                {
                    return seq[ctx.Index];
                }
            }
            else if (ctx.Node.Kind == NodeKind.Scalar)
            {
                if (ctx.Index == 0)
                {
                    return ctx.Node;
                }
            }

            contexts.RemoveAt(count - 1);
            count = contexts.Count;
        }

        return null;
    }

    Node? NextValue()
    {
        var count = contexts.Count;
        while (count > 0)
        {
            var ctx = contexts[count - 1];
            if (ctx.Node.Kind == NodeKind.Mapping)
            {
                var map = ctx.Node.Map;
                if (map != null && ctx.Index < map.Count)
                {
                    var value = map[ctx.Index].Value;
                    ctx.Index += 1;
                    contexts[count - 1] = ctx;
                    return value;
                }
            }
            else if (ctx.Node.Kind == NodeKind.Sequence)
            {
                var seq = ctx.Node.Sequence;
                if (seq != null && ctx.Index < seq.Count)
                {
                    var value = seq[ctx.Index];
                    ctx.Index += 1;
                    contexts[count - 1] = ctx;
                    return value;
                }
            }
            else if (ctx.Node.Kind == NodeKind.Scalar)
            {
                if (ctx.Index == 0)
                {
                    ctx.Index = 1;
                    contexts[count - 1] = ctx;
                    return ctx.Node;
                }
            }

            contexts.RemoveAt(count - 1);
            count = contexts.Count;
        }

        return null;
    }

    public string? ReadString()
    {
        if (contexts.Count > 0)
        {
            var ctx = contexts[contexts.Count - 1];
            if (ctx.UseNode && ctx.Node.Kind != NodeKind.Scalar)
            {
                return null;
            }
        }

        var node = PeekValue();
        if (node == null)
        {
            return null;
        }

        if (node.Value.Kind == NodeKind.Scalar)
        {
            node = NextValue();
            return node?.Scalar;
        }

        return null;
    }

    public T Deserialize<T>() where T : notnull
    {
        Node? node = null;

        node ??= TakeContextNode();
        node ??= NextValue();

        if (node == null)
        {
            return default!;
        }

        var serializer = SerializerLookup<WorldSerializationContext>.GetSerializer<T>();
        if (serializer != null)
        {
            var useNode = ShouldUseContextNode(typeof(T), node.Value);
            PushContext(node.Value, useNode);
            var value = serializer.Deserialize(ref this, in context);
            PopContext();
            return value;
        }

        var defaultSerializer = SerializerLookup<DefaultSerializationContext>.GetSerializer<T>();
        if (defaultSerializer != null)
        {
            var useNode = ShouldUseContextNode(typeof(T), node.Value);
            PushContext(node.Value, useNode);
            var value = defaultSerializer.Deserialize(ref this, new DefaultSerializationContext());
            PopContext();
            return value;
        }

        return default!;
    }

    public T Read<T>() where T : unmanaged
    {
        var node = NextValue();
        if (node == null)
        {
            return default;
        }

        var blittableSerializer = BlittableSerializerLookup<WorldSerializationContext>.GetSerializer<T>();
        if (blittableSerializer != null)
        {
            var useNode = ShouldUseContextNode(typeof(T), node.Value);
            PushContext(node.Value, useNode);
            var value = blittableSerializer.Deserialize(ref this, in context);
            PopContext();
            return value;
        }

        var defaultSerializer = BlittableSerializerLookup<DefaultSerializationContext>.GetSerializer<T>();
        if (defaultSerializer != null)
        {
            var useNode = ShouldUseContextNode(typeof(T), node.Value);
            PushContext(node.Value, useNode);
            var value = defaultSerializer.Deserialize(ref this, new DefaultSerializationContext());
            PopContext();
            return value;
        }

        if (node.Value.Kind == NodeKind.Scalar)
        {
            return ParseScalar<T>(node.Value.Scalar);
        }

        return default;
    }

    public T[] ReadArray<T>() where T : unmanaged
    {
        var node = NextValue();
        if (node == null)
        {
            return Array.Empty<T>();
        }

        if (node.Value.Kind == NodeKind.Sequence && node.Value.Sequence != null)
        {
            var seq = node.Value.Sequence;
            var arr = new T[seq.Count];
            for (int i = 0; i < seq.Count; i++)
            {
                var element = seq[i];
                if (element.Kind == NodeKind.Scalar)
                {
                    arr[i] = ParseScalar<T>(element.Scalar);
                }
                else
                {
                    var serializer = BlittableSerializerLookup<WorldSerializationContext>.GetSerializer<T>();
                    if (serializer != null)
                    {
                        var useNode = ShouldUseContextNode(typeof(T), element);
                        PushContext(element, useNode);
                        arr[i] = serializer.Deserialize(ref this, in context);
                        PopContext();
                    }
                }
            }
            return arr;
        }

        return Array.Empty<T>();
    }

    public void Dispose()
    {
        if (types != null)
        {
            types.Clear();
            Pool<Dictionary<string, Type>>.Shared.Return(types);
            types = null!;
        }
        contexts?.Clear();
    }

    Node ParseDocument()
    {
        var lines = new List<(int start, int length, int indent)>();
        var minIndent = int.MaxValue;
        while (cursor < length)
        {
            var lineStart = cursor;
            var lineSpan = ReadLine();
            if (lineSpan.Length == 0 && cursor >= length)
            {
                break;
            }

            var indent = CountIndent(lineSpan);
            var trimmed = TrimSpace(lineSpan);
            if (trimmed.Length > 0 && trimmed[0] != (byte)'#')
            {
                minIndent = Math.Min(minIndent, indent);
            }
            lines.Add((lineStart, lineSpan.Length, indent));
        }

        if (minIndent == int.MaxValue)
        {
            minIndent = 0;
        }

        var adjusted = new List<(int start, int length, int indent)>(lines.Count);
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var removed = Math.Min(line.indent, minIndent);
            adjusted.Add((line.start + removed, Math.Max(0, line.length - removed), Math.Max(0, line.indent - minIndent)));
        }

        var lineIndex = 0;
        return ParseMapping(adjusted, ref lineIndex, 0);
    }

    Node ParseMapping(List<(int start, int length, int indent)> lines, ref int lineIndex, int indent)
    {
        var entries = new List<KeyValuePair<string, Node>>();

        while (lineIndex < lines.Count)
        {
            var lineInfo = lines[lineIndex];
            if (lineInfo.length == 0)
            {
                lineIndex += 1;
                continue;
            }

            if (lineInfo.indent < indent)
            {
                break;
            }

            if (lineInfo.indent > indent)
            {
                break;
            }

            var line = data.Slice(lineInfo.start, lineInfo.length);
            var trimmed = line[lineInfo.indent..];
            if (trimmed.Length == 0)
            {
                lineIndex += 1;
                continue;
            }

            if (trimmed[0] == (byte)('#'))
            {
                lineIndex += 1;
                continue;
            }

            var colonIndex = trimmed.IndexOf((byte)':');
            if (colonIndex < 0)
            {
                lineIndex += 1;
                continue;
            }

            var keySpan = trimmed.Slice(0, colonIndex);
            var valueSpan = TrimSpace(trimmed.Slice(colonIndex + 1));
            var key = Encoding.UTF8.GetString(keySpan);

            if (valueSpan.Length == 0)
            {
                lineIndex += 1;
                var child = ParseMapping(lines, ref lineIndex, indent + 2);
                entries.Add(new KeyValuePair<string, Node>(key, child));
                continue;
            }

            Node value;
            if (valueSpan[0] == (byte)'{')
            {
                value = ParseInlineObject(valueSpan);
            }
            else if (valueSpan[0] == (byte)'[')
            {
                value = ParseInlineArray(valueSpan);
            }
            else
            {
                value = new Node
                {
                    Kind = NodeKind.Scalar,
                    Scalar = DecodeScalar(valueSpan),
                };
            }

            entries.Add(new KeyValuePair<string, Node>(key, value));
            lineIndex += 1;
        }

        return new Node
        {
            Kind = NodeKind.Mapping,
            Map = entries,
        };
    }

    Node ParseInlineObject(ReadOnlySpan<byte> span)
    {
        var inner = TrimBraces(span);
        var entries = new List<KeyValuePair<string, Node>>();
        var idx = 0;
        while (idx < inner.Length)
        {
            SkipInlineSeparators(inner, ref idx);
            if (idx >= inner.Length)
            {
                break;
            }
            var keyStart = idx;
            while (idx < inner.Length && inner[idx] != (byte)':')
            {
                idx += 1;
            }
            var keySpan = inner.Slice(keyStart, idx - keyStart);
            idx += 1;
            SkipInlineSeparators(inner, ref idx);
            var valueSpan = ReadInlineValue(inner, ref idx);
            var key = DecodeScalar(TrimSpace(keySpan));
            var value = ParseInlineValue(valueSpan.ToArray());
            entries.Add(new KeyValuePair<string, Node>(key, value));
        }

        return new Node
        {
            Kind = NodeKind.Mapping,
            Map = entries,
        };
    }

    Node ParseInlineArray(ReadOnlySpan<byte> span)
    {
        var inner = TrimBrackets(span);
        var seq = new List<Node>();
        var idx = 0;
        while (idx < inner.Length)
        {
            SkipInlineSeparators(inner, ref idx);
            if (idx >= inner.Length)
            {
                break;
            }
            var valueSpan = ReadInlineValue(inner, ref idx);
            seq.Add(ParseInlineValue(valueSpan.ToArray()));
        }

        return new Node
        {
            Kind = NodeKind.Sequence,
            Sequence = seq,
        };
    }

    Node ParseInlineValue(ReadOnlySpan<byte> span)
    {
        var trimmed = TrimSpace(span);
        if (trimmed.Length == 0)
        {
            return new Node { Kind = NodeKind.Scalar, Scalar = string.Empty };
        }

        if (trimmed[0] == (byte)'{')
        {
            return ParseInlineObject(trimmed);
        }

        if (trimmed[0] == (byte)'[')
        {
            return ParseInlineArray(trimmed);
        }

        return new Node
        {
            Kind = NodeKind.Scalar,
            Scalar = DecodeScalar(trimmed),
        };
    }

    ReadOnlySpan<byte> TrimSpace(ReadOnlySpan<byte> span)
    {
        var start = 0;
        var end = span.Length - 1;
        while (start <= end && IsSpace(span[start]))
        {
            start += 1;
        }
        while (end >= start && IsSpace(span[end]))
        {
            end -= 1;
        }
        return span.Slice(start, end - start + 1);
    }

    ReadOnlySpan<byte> TrimBraces(ReadOnlySpan<byte> span)
    {
        var trimmed = TrimSpace(span);
        if (trimmed.Length >= 2 && trimmed[0] == (byte)'{' && trimmed[^1] == (byte)'}')
        {
            return trimmed.Slice(1, trimmed.Length - 2);
        }
        return trimmed;
    }

    ReadOnlySpan<byte> TrimBrackets(ReadOnlySpan<byte> span)
    {
        var trimmed = TrimSpace(span);
        if (trimmed.Length >= 2 && trimmed[0] == (byte)'[' && trimmed[^1] == (byte)']')
        {
            return trimmed.Slice(1, trimmed.Length - 2);
        }
        return trimmed;
    }

    void SkipInlineSeparators(ReadOnlySpan<byte> span, ref int idx)
    {
        while (idx < span.Length)
        {
            var c = span[idx];
            if (c == (byte)',' || IsSpace(c))
            {
                idx += 1;
                continue;
            }
            break;
        }
    }

    ReadOnlySpan<byte> ReadInlineValue(ReadOnlySpan<byte> span, ref int idx)
    {
        var start = idx;
        var depthBrace = 0;
        var depthBracket = 0;
        var inString = false;
        while (idx < span.Length)
        {
            var c = span[idx];
            if (c == (byte)'"')
            {
                inString = !inString;
            }
            else if (!inString)
            {
                if (c == (byte)'{')
                {
                    depthBrace += 1;
                }
                else if (c == (byte)'}')
                {
                    depthBrace -= 1;
                }
                else if (c == (byte)'[')
                {
                    depthBracket += 1;
                }
                else if (c == (byte)']')
                {
                    depthBracket -= 1;
                }
                else if (c == (byte)',' && depthBrace == 0 && depthBracket == 0)
                {
                    break;
                }
            }

            idx += 1;
            if (depthBrace == 0 && depthBracket == 0 && !inString && idx < span.Length && span[idx] == (byte)',' && start != idx)
            {
                break;
            }
        }

        return span.Slice(start, idx - start);
    }

    ReadOnlySpan<byte> ReadLine()
    {
        if (cursor >= length)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        var start = cursor;
        while (cursor < length && data[cursor] != (byte)'\n')
        {
            cursor += 1;
        }

        var end = cursor;
        if (cursor < length && data[cursor] == (byte)'\n')
        {
            cursor += 1;
        }

        if (end > start && data[end - 1] == (byte)'\r')
        {
            end -= 1;
        }

        return data.Slice(start, end - start);
    }

    int CountIndent(ReadOnlySpan<byte> line)
    {
        var count = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == (byte)' ')
            {
                count += 1;
            }
            else
            {
                break;
            }
        }
        return count;
    }

    bool IsSpace(byte c)
    {
        return c == (byte)' ' || c == (byte)'\t';
    }

    bool ShouldUseContextNode(Type type, Node node)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Handle<>) && node.Kind != NodeKind.Scalar;
    }

    string DecodeScalar(ReadOnlySpan<byte> span)
    {
        var trimmed = TrimSpace(span);
        if (trimmed.Length >= 2 && trimmed[0] == (byte)'"' && trimmed[^1] == (byte)'"')
        {
            trimmed = trimmed.Slice(1, trimmed.Length - 2);
        }
        return Encoding.UTF8.GetString(trimmed);
    }

    int ParseInt(string? text)
    {
        if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }
        return 0;
    }

    T ParseScalar<T>(string? text) where T : unmanaged
    {
        if (typeof(T) == typeof(int))
        {
            var v = int.Parse(text ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);
            return Unsafe.As<int, T>(ref v);
        }
        if (typeof(T) == typeof(float))
        {
            var v = float.Parse(text ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);
            return Unsafe.As<float, T>(ref v);
        }
        if (typeof(T) == typeof(double))
        {
            var v = double.Parse(text ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);
            return Unsafe.As<double, T>(ref v);
        }
        if (typeof(T) == typeof(uint))
        {
            var v = uint.Parse(text ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);
            return Unsafe.As<uint, T>(ref v);
        }
        if (typeof(T).IsEnum)
        {
            if (Enum.TryParse(typeof(T), text ?? string.Empty, true, out var parsed))
            {
                return (T)parsed!;
            }

            if (typeof(T) == typeof(ShaderStage))
            {
                var v = ShaderStage.Fragment;
                return Unsafe.As<ShaderStage, T>(ref v);
            }

            return default;
        }

        return default;
    }
}