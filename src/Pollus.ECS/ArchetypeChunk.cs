namespace Pollus.ECS;

using Pollus.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public struct ArchetypeChunk : IDisposable
{
    internal NativeMap<int, NativeArray<byte>> components;
    internal NativeArray<Entity> entities;

    int count;
    int length;

    public int Count => count;
    public int Length => length;

    public ArchetypeChunk(scoped in Span<ComponentID> cids, int rows = 0)
    {
        length = rows;
        count = 0;
        entities = new(rows);
        components = new(cids.Length);

        foreach (var cid in cids)
        {
            var cinfo = Component.GetInfo(cid);
            components.Add(cid, new(rows * cinfo.SizeInBytes));
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
        
        var entity = entities[row];
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