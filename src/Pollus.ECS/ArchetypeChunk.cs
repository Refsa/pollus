namespace Pollus.ECS;

using Pollus.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[Flags]
public enum ComponentFlags : byte
{
    None = 0,
    Added = 1 << 0,
    Changed = 1 << 1,
}

public struct ArchetypeChunk : IDisposable
{
    internal NativeMap<int, NativeArray<byte>> components;
    internal NativeArray<Entity> entities;

    NativeMap<int, ComponentFlags> flags;
    int firstFlagIndex;
    int lastFlagIndex;

    int count;
    int length;

    public int Count => count;
    public int Length => length;
    public Range FlagRange => firstFlagIndex..lastFlagIndex;

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
            flags.Add(cid, ComponentFlags.None);
        }
    }

    public void Dispose()
    {
        foreach (var value in components.Values)
        {
            value.Dispose();
        }
        components.Dispose();
        entities.Dispose();
    }

    public bool HasFlag<C>(ComponentFlags flag)
        where C : unmanaged, IComponent
    {
        return flags.Get(Component.GetInfo<C>().ID).HasFlag(flag);
    }

    public void ClearFlags()
    {
        foreach (ref var value in flags.Values)
        {
            value = ComponentFlags.None;
        }
        firstFlagIndex = 0;
        lastFlagIndex = 0;
    }

    void SetFlag<C>(ComponentFlags flag, int row)
        where C : unmanaged, IComponent
    {
        SetFlag(Component.GetInfo<C>().ID, flag, row);
    }

    void SetFlag(ComponentID cid, ComponentFlags flag, int row)
    {
        firstFlagIndex = int.Min(firstFlagIndex, row);
        lastFlagIndex = int.Max(lastFlagIndex, row);
        flags.Get(cid) |= flag;
    }

    void SetAllFlags(ComponentFlags flag, int row)
    {
        firstFlagIndex = int.Min(firstFlagIndex, row);
        lastFlagIndex = int.Max(lastFlagIndex, row);
        foreach (ref var value in flags.Values)
        {
            value |= flag;
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
        SetAllFlags(ComponentFlags.Added, count);
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
    unsafe public Span<Entity> GetEntities()
    {
        return new Span<Entity>(entities.Data, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public Span<C> GetComponents<C>()
            where C : unmanaged, IComponent
    {
        var cinfo = Component.GetInfo<C>();
        var array = components.Get(cinfo.ID);
        return new Span<C>(array.Data, count);
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
        var cid = Component.GetInfo<C>().ID;
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