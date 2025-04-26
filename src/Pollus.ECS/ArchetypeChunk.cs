namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    internal NativeArray<NativeArray<byte>> components;
    internal NativeArray<Entity> entities;

    ulong version;
    NativeMap<int, int> componentsLookup;
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
        componentsLookup = new(cids.Length);
        changes = new(cids.Length);

        int idx = 0;
        foreach (var cid in cids)
        {
            var cinfo = Component.GetInfo(cid);
            componentsLookup.Add(cid.ID, idx);
            components[idx] = new(rows * cinfo.SizeInBytes);
            changes[idx] = new ChunkComponentInfo(idx, version, rows);
            idx++;
        }
    }

    public ArchetypeChunk(scoped in Span<ComponentID> cids, scoped in Span<NativeArray<byte>> componentMemory, int rows = 0)
    {
        length = rows;
        count = 0;
        entities = new(rows);
        components = new(cids.Length);
        componentsLookup = new(cids.Length);
        changes = new(cids.Length);

        int idx = 0;
        foreach (var cid in cids)
        {
            componentsLookup.Add(cid.ID, idx);
            components[idx] = componentMemory[idx];
            changes[idx] = new ChunkComponentInfo(idx, version, rows);
            idx++;
        }
    }

    public void Dispose()
    {
        foreach (var value in components) value.Dispose();
        foreach (var value in changes) value.Dispose();

        components.Dispose();
        entities.Dispose();
        componentsLookup.Dispose();
    }

    internal void Tick(ulong version)
    {
        this.version = version;
    }

    public bool HasFlag<C>(int row, ComponentFlags flag)
        where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        ref var idx = ref componentsLookup.Get(cid);
        if (Unsafe.IsNullRef(ref idx)) return false;

        if (!changes[idx].CheckFlag(row, flag, version)) return false;
        return true;
    }

    internal void SetFlag<C>(int row, in ComponentFlags flag)
        where C : unmanaged, IComponent
    {
        SetFlag(row, Component.GetInfo<C>().ID, flag);
    }

    internal void SetFlag(int row, in ComponentID cid, ComponentFlags flag)
    {
        scoped ref var idx = ref componentsLookup.Get(cid.ID);
        if (flag == ComponentFlags.Removed)
        {
            if (Unsafe.IsNullRef(ref idx))
            {
                changes.Resize(changes.Length + 1);
                changes[^1] = new ChunkComponentInfo(componentsLookup.Count, version, 0);
                componentsLookup.Add(cid.ID, changes.Length - 1);
            }
            else changes[idx].SetFlag(-1, flag, version);
            return;
        }

        changes[idx].SetFlag(row, flag, version);
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
    unsafe public Entity SwapRemoveEntity(int dstRow, scoped ref ArchetypeChunk src)
    {
        var srcIndex = --src.count;
        entities[dstRow] = src.entities[srcIndex];
        src.entities[srcIndex] = Entity.NULL;

        foreach (var cid in componentsLookup.Keys)
        {
            var idx = componentsLookup.Get(cid);
            var size = Component.GetInfo(cid).SizeInBytes;

            ref var srcArray = ref src.components[idx];
            ref var dstArray = ref components[idx];

            var srcRowData = Unsafe.Add<byte>(srcArray.Data, srcIndex * size);
            var dstRowData = Unsafe.Add<byte>(dstArray.Data, dstRow * size);

            Unsafe.CopyBlock(dstRowData, srcRowData, (uint)size);
        }

        return entities[dstRow];
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
        var idx = componentsLookup.Get(cid.ID);
        return new Span<C>(components[idx].Data, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public NativeArray<C> GetComponentsNative<C>(ComponentID cid)
            where C : unmanaged, IComponent
    {
        var idx = componentsLookup.Get(cid.ID);
        return Unsafe.As<NativeArray<byte>, NativeArray<C>>(ref components[idx]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public void SetComponent<C>(int row, scoped in C component)
            where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        var idx = componentsLookup.Get(cid.ID);
        ref var array = ref components[idx];
        Unsafe.Write(Unsafe.Add<C>(array.Data, row), component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public void SetComponent(int row, in ComponentID cid, scoped in ReadOnlySpan<byte> component)
    {
        var idx = componentsLookup.Get(cid.ID);
        ref var array = ref components[idx];
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
        var idx = componentsLookup.Get(cid.ID);
        var array = components[idx];
        return ref Unsafe.AsRef<C>(Unsafe.Add<C>(array.Data, row));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    unsafe public Span<byte> GetComponent(int row, in ComponentID cid)
    {
        var idx = componentsLookup.Get(cid.ID);
        var cinfo = Component.GetInfo(cid);
        var array = components[idx];
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
        return HasComponent(Component.GetInfo<C>().ID);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasComponent(ComponentID cid)
    {
        return componentsLookup.Has(cid.ID);
    }
}