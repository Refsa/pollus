namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Collections;

struct ChunkComponentInfo : IDisposable
{
    [InlineArray(3)]
    public struct ChangeVersions
    {
        ulong _first;
    }

    int componentIndex;
    ChangeVersions changes;
    NativeArray<ChangeVersions> rowChanges;

    public int ComponentIndex => componentIndex;

    public ChunkComponentInfo(int componentIndex, ulong version, int rows)
    {
        this.componentIndex = componentIndex;
        changes[0] = changes[1] = changes[2] = version;

        if (rows > 0) rowChanges = new(rows);
    }

    public void Dispose()
    {
        if (rowChanges.Length > 0) rowChanges.Dispose();
    }

    public bool CheckFlag(int row, ComponentFlags flag, ulong version)
    {
        if (!(version - changes[(int)flag] <= 1)) return false;
        if (row < 0) return true;
        return version - rowChanges[row][(int)flag] <= 1;
    }

    public void SetFlag(int row, ComponentFlags flag, ulong version)
    {
        changes[(int)flag] = version;
        if (row >= 0) rowChanges[row][(int)flag] = version;
    }
}

public struct ArchetypeChunk : IDisposable
{
    internal NativeMap<int, NativeArray<byte>> components;
    internal NativeArray<Entity> entities;

    ulong version;
    NativeMap<int, int> changesLookup;
    NativeArray<ChunkComponentInfo> changes;

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
        changesLookup = new(cids.Length);
        changes = new(cids.Length);

        int idx = 0;
        foreach (var cid in cids)
        {
            var cinfo = Component.GetInfo(cid);
            components.Add(cid.ID, new(rows * cinfo.SizeInBytes));

            changesLookup.Add(cid.ID, idx);
            changes[idx] = new ChunkComponentInfo(idx, version, rows);
            idx++;
        }
    }

    public void Dispose()
    {
        foreach (var value in components.Values) value.Dispose();
        components.Dispose();
        entities.Dispose();

        foreach (var value in changes) value.Dispose();
        changesLookup.Dispose();
    }

    internal void Tick(ulong version)
    {
        this.version = version;
    }

    public bool HasFlag<C>(int row, ComponentFlags flag)
        where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        ref var info = ref changesLookup.Get(cid);
        if (Unsafe.IsNullRef(ref info)) return false;

        if (!changes[info].CheckFlag(row, flag, version)) return false;
        return true;
    }

    internal void SetFlag<C>(int row, in ComponentFlags flag)
        where C : unmanaged, IComponent
    {
        SetFlag(row, Component.GetInfo<C>().ID, flag);
    }

    internal void SetFlag(int row, in ComponentID cid, ComponentFlags flag)
    {
        scoped ref var info = ref changesLookup.Get(cid.ID);
        if (flag == ComponentFlags.Removed)
        {
            if (Unsafe.IsNullRef(ref info))
            {
                changes.Resize(changes.Length + 1);
                changes[^1] = new ChunkComponentInfo(changesLookup.Count, version, 0);
                changesLookup.Add(cid.ID, changes.Length - 1);
            }
            else changes[info].SetFlag(-1, flag, version);
            return;
        }

        changes[info].SetFlag(row, flag, version);
    }

    internal void SetAllFlags(int row, ComponentFlags flag)
    {
        foreach (ref var value in changes)
        {
            value.SetFlag(row, flag, version);
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
        count = int.Max(0, count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public Entity SwapRemoveEntity(int row, scoped ref ArchetypeChunk src)
    {
        /*
        remove entity in row from this chunk
        move entity in last row from src chunk to this chunk
        return the moved entity
        */

        entities[row] = src.entities[--src.count];
        src.entities[src.count] = Entity.NULL;

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
        return new Span<C>(components.Get(cid.ID).Data, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public void SetComponent<C>(int row, scoped in C component)
            where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        ref var array = ref components.Get(cid.ID);
        Unsafe.Write(Unsafe.Add<C>(array.Data, row), component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public void SetComponent(int row, in ComponentID cid, scoped in ReadOnlySpan<byte> component)
    {
        ref var array = ref components.Get(cid.ID);
        var dst = Unsafe.Add<byte>(array.Data, row * component.Length);
        component.CopyTo(new Span<byte>(dst, component.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public ref C GetComponent<C>(int row)
            where C : unmanaged, IComponent
    {
        var cinfo = Component.GetInfo<C>();
        return ref GetComponent<C>(row, cinfo.ID);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public ref C GetComponent<C>(int row, scoped in ComponentID cid)
            where C : unmanaged, IComponent
    {
        var array = components.Get(cid.ID);
        return ref *(C*)Unsafe.Add<C>(array.Data, row);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public Span<byte> GetComponent(int row, in ComponentID cid)
    {
        var cinfo = Component.GetInfo(cid);
        var array = components.Get(cid.ID);
        return new Span<byte>(Unsafe.Add<byte>(array.Data, row * cinfo.SizeInBytes), cinfo.SizeInBytes);
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
        return components.Has(Component.GetInfo<C>().ID.ID);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasComponent(ComponentID cid)
    {
        return components.Has(cid.ID);
    }
}