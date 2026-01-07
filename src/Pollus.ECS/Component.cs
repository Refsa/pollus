namespace Pollus.ECS;

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public interface IComponent
{
    static virtual void CollectRequired(Dictionary<ComponentID, byte[]> collector) { }
}

public interface IDefault<C>
    where C : unmanaged, IComponent
{
    static virtual C Default { get; } = default;
}

[DebuggerDisplay("{Info.TypeName}")]
public readonly record struct ComponentID(int ID)
{
    public static implicit operator int(ComponentID cid) => cid.ID;
    public static implicit operator ComponentID(int id) => new(id);

    public override int GetHashCode() => ID;

    public Component.Info Info => Component.GetInfo(ID);

    public override string ToString() => $"ComponentID({ID}, {Info.TypeName})";
}

public static class Component
{
    public record Info
    {
        public required ComponentID ID { get; init; }
        public required int SizeInBytes { get; init; }
        public required Type Type { get; init; }
        public required string TypeName { get; init; }
        public required bool Read { get; init; }
        public required bool Write { get; init; }

        public Action<RemovedTracker>? RegisterTracker { get; internal set; }
    }

    static class Lookup<C> where C : unmanaged, IComponent
    {
        public static readonly Info Info;

        static Lookup()
        {
            Info = Register<C>();
        }
    }

    static readonly ConcurrentDictionary<Type, Info> componentIDs = new();
    static readonly ConcurrentDictionary<ComponentID, Info> componentInfos = new();

    public static ReadOnlyDictionary<ComponentID, Info> ComponentInfos => new(componentInfos);

    public static Info Register(Info info)
    {
        if (componentIDs.TryGetValue(info.Type, out var existing))
            return existing;

        if (info.ID == -1) info = info with { ID = new ComponentID(componentIDs.Count) };

        componentIDs[info.Type] = info;
        componentInfos[info.ID] = info;

        return info;
    }

    public static Info Register<T>() where T : unmanaged, IComponent
    {
        var type = typeof(T);
        if (componentIDs.TryGetValue(type, out var info))
        {
            info.RegisterTracker ??= static (removedTracker) => removedTracker.Register<T>();
            return info;
        }

        info = new Info
        {
            ID = new ComponentID(componentIDs.Count),
            SizeInBytes = Unsafe.SizeOf<T>(),
            Type = type,
            TypeName = type.AssemblyQualifiedName ?? type.FullName ?? throw new InvalidOperationException($"Type {type} has no assembly qualified name"),
            Read = true,
            Write = true,
            RegisterTracker = static (removedTracker) => removedTracker.Register<T>(),
        };

        if (new T() is IComponentWrapper)
        {
#pragma warning disable IL2059
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
#pragma warning restore IL2059
            info = ComponentWrapper<T>.Info;
        }

        componentIDs[type] = info;
        componentInfos[info.ID] = info;

        return info;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Info GetInfo(Type type)
    {
        return componentIDs[type];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Info GetInfo(ComponentID cid)
    {
        return componentInfos[cid];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Info GetInfo<T>() where T : unmanaged, IComponent
    {
        return Lookup<T>.Info;
    }
}
