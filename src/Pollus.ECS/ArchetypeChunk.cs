namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Collections;

struct ArchetypeChunkInfo : IDisposable
{
    public int FirstFlagIndex;
    public int LastFlagIndex;
    public ulong AddedVersion;
    public ulong ChangedVersion;
    public ulong RemovedVersion;
    public NativeArray<ArchetypeChunkComponentInfo> Changes;

    public void Dispose()
    {
        Changes.Dispose();
    }

    public bool CheckFlag(ComponentFlags flag, ulong version)
    {
        return flag switch
        {
            ComponentFlags.Added => (version - AddedVersion) <= 1,
            ComponentFlags.Changed => (version - ChangedVersion) <= 1,
            ComponentFlags.Removed => (version - RemovedVersion) <= 1,
            _ => false
        };
    }

    public void SetFlag(ComponentFlags flag, ulong version)
    {
        if (flag == ComponentFlags.Added) AddedVersion = version;
        else if (flag == ComponentFlags.Changed) ChangedVersion = version;
        else if (flag == ComponentFlags.Removed) RemovedVersion = version;
    }
}

struct ArchetypeChunkComponentInfo
{
    public ulong AddedVersion;
    public ulong ChangedVersion;

    public bool CheckFlag(ComponentFlags flag, ulong version)
    {
        return flag switch
        {
            ComponentFlags.Added => (version - AddedVersion) <= 1,
            ComponentFlags.Changed => (version - ChangedVersion) <= 1,
            _ => false
        };
    }

    public void SetFlag(ComponentFlags flag, ulong version)
    {
        if (flag == ComponentFlags.Added) AddedVersion = version;
        else if (flag == ComponentFlags.Changed) ChangedVersion = version;
    }
}

public struct ArchetypeChunk : IDisposable
{
    internal NativeMap<int, NativeArray<byte>> components;
    internal NativeArray<Entity> entities;

    ulong version;
    NativeMap<int, ArchetypeChunkInfo> flags;

    int count;
    readonly int length;

    public readonly int Count => count;
    public readonly int Length => length;

    public ArchetypeChunk(scoped in Span<ComponentID> cids, int rows = 0)
    {
        length = rows;
        count = 0;
        entities = new(rows);
        components = new(cids.Length);
        flags = new(cids.Length);

        foreach (var cid in cids)
        {
            var cinfo = Component.GetInfo(cid);
            components.Add(cid, new(rows * cinfo.SizeInBytes));
            flags.Add(cid, new ArchetypeChunkInfo { AddedVersion = version, ChangedVersion = version, Changes = new(rows) });
        }
    }

    public void Dispose()
    {
        foreach (var value in components.Values) value.Dispose();
        components.Dispose();
        entities.Dispose();

        foreach (var value in flags.Values) value.Dispose();
        flags.Dispose();
    }

    internal void Tick(ulong version)
    {
        this.version = version;
    }

    public bool HasFlag<C>(int row, ComponentFlags flag)
        where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        ref var info = ref flags.Get(cid);
        if (Unsafe.IsNullRef(ref info)) return false;

        if (!info.CheckFlag(flag, version)) return false;
        if (flag != ComponentFlags.Removed && row >= 0 && !info.Changes[row].CheckFlag(flag, version)) return false;
        return true;
    }

    internal void SetFlag<C>(int row, ComponentFlags flag)
        where C : unmanaged, IComponent
    {
        SetFlag(row, Component.GetInfo<C>().ID, flag);
    }

    internal void SetFlag(int row, ComponentID cid, ComponentFlags flag)
    {
        ref var info = ref flags.Get(cid);
        if (Unsafe.IsNullRef(ref info))
        {
            flags.Add(cid, new ArchetypeChunkInfo { RemovedVersion = version });
            info = ref flags.Get(cid);
        }

        if (flag != ComponentFlags.Removed)
        {
            info.SetFlag(flag, version);
            info.Changes[row].SetFlag(flag, version);
        }

        info.FirstFlagIndex = int.Min(info.FirstFlagIndex, row);
        info.LastFlagIndex = int.Max(info.LastFlagIndex, row);
    }

    internal void SetAllFlags(ComponentFlags flag, int row)
    {
        foreach (var cid in components.Keys)
        {
            ref var value = ref flags.Get(cid);
            value.SetFlag(flag, version);
            value.FirstFlagIndex = int.Min(value.FirstFlagIndex, row);
            value.LastFlagIndex = int.Max(value.LastFlagIndex, row);
            value.Changes[row].SetFlag(flag, version);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasEntity(in Entity entity)
    {
        for (int i = 0; i < count; i++)
        {
            if (entities[i] == entity) return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int AddEntity(in Entity entity)
    {
        if (count >= length) return -1;

        entities[count] = entity;
        return count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void RemoveEntity(int row)
    {
        if (row < 0 || row >= count) return;

        entities[row] = Entity.NULL;
        count--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public Entity SwapRemoveEntity(int row, scoped ref ArchetypeChunk src)
    {
        /*
        remove entity in row from this chunk
        move entity in last row from src chunk to this chunk
        return the moved entity
        */

        entities[row] = src.entities[src.count - 1];
        src.entities[src.count - 1] = Entity.NULL;
        src.count--;

        foreach (var cid in components.Keys)
        {
            var size = Component.GetInfo(cid).SizeInBytes;

            var srcArray = src.components.Get(cid);
            var dstArray = components.Get(cid);
            var srcRow = Unsafe.Add<byte>(srcArray.Data, src.count * size);
            var dstRow = Unsafe.Add<byte>(dstArray.Data, row * size);
            Unsafe.CopyBlock(dstRow, srcRow, (uint)size);
        }

        return entities[row];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public readonly ReadOnlySpan<Entity> GetEntities()
    {
        return new ReadOnlySpan<Entity>(entities.Data, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public readonly ref Entity GetEntity(int row)
    {
        return ref entities[row];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public Span<C> GetComponents<C>(ComponentID cid)
            where C : unmanaged, IComponent
    {
        return new Span<C>(components.Get(cid).Data, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public void SetComponent<C>(int row, scoped in C component)
            where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        ref var array = ref components.Get(cid);
        Unsafe.Write(Unsafe.Add<C>(array.Data, row), component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public void SetComponent(int row, in ComponentID cid, scoped in ReadOnlySpan<byte> component)
    {
        ref var array = ref components.Get(cid);
        var dst = Unsafe.Add<byte>(array.Data, row * component.Length);
        component.CopyTo(new Span<byte>(dst, component.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public ref C GetComponent<C>(int row)
            where C : unmanaged, IComponent
    {
        return ref GetComponent<C>(row, Component.GetInfo<C>().ID);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public ref C GetComponent<C>(int row, scoped in ComponentID cid)
            where C : unmanaged, IComponent
    {
        var array = components.Get(cid);
        return ref *(C*)Unsafe.Add<C>(array.Data, row);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public Span<byte> GetComponent(int row, in ComponentID cid)
    {
        var array = components.Get(cid);
        return new Span<byte>(Unsafe.Add<byte>(array.Data, row * Component.GetInfo(cid).SizeInBytes), Component.GetInfo(cid).SizeInBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetCount(int newCount)
    {
        count = newCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasComponent<C>()
            where C : unmanaged, IComponent
    {
        return components.Has(Component.GetInfo<C>().ID);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasComponent(ComponentID cid)
    {
        return components.Has(cid);
    }
}