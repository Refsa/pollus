namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public struct ArchetypeChunk : IDisposable
{
    NativeMap<int, NativeArray<byte>> components;
    NativeArray<Entity> entities;

    int count;
    int length;

    public int Count => count;
    public int Length => length;

    public ArchetypeChunk(Span<ComponentID> cids, int rows = 0)
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

    public int AddEntity(in Entity entity)
    {
        if (count >= length) return -1;

        entities[count] = entity;
        return count++;
    }

    unsafe public Entity SwapRemoveEntity(int row, ref ArchetypeChunk source)
    {
        for (int i = 0; i < components.Count; i++)
        {
            var cid = components.Keys[i];
            if (source.HasComponent(cid) is false) continue;

            var cinfo = Component.GetInfo(cid);
            var srcArray = source.components.Get(cid);
            var dstArray = components.Get(cid);

            Unsafe.CopyBlock(Unsafe.Add<byte>(dstArray.Data, row * cinfo.SizeInBytes),
                             Unsafe.Add<byte>(srcArray.Data, (source.count - 1) * cinfo.SizeInBytes),
                             (uint)cinfo.SizeInBytes);
        }

        entities[row] = source.entities[source.count - 1];
        source.count--;

        return entities[row];
    }

    unsafe public Span<Entity> GetEntities()
    {
        return new Span<Entity>(entities.Data, count);
    }

    unsafe public Span<C> GetComponents<C>()
        where C : unmanaged, IComponent
    {
        var cinfo = Component.GetInfo<C>();
        var array = components.Get(cinfo.ID);
        return new Span<C>(array.Data, count);
    }

    unsafe public void SetComponent<C>(int row, in C component)
        where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        ref var array = ref components.Get(cid);
        Unsafe.Write(Unsafe.Add<C>(array.Data, row), component);
    }

    unsafe public void SetComponent(int row, ComponentID cid, in ReadOnlySpan<byte> component)
    {
        ref var array = ref components.Get(cid);
        var dst = Unsafe.Add<byte>(array.Data, row * component.Length);
        component.CopyTo(new Span<byte>(dst, component.Length));
    }

    unsafe public ref C GetComponent<C>(int row)
        where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        var array = components.Get(cid);
        return ref *(C*)Unsafe.Add<C>(array.Data, row);
    }

    unsafe public Span<byte> GetComponent(int row, ComponentID cid)
    {
        var array = components.Get(cid);
        return new Span<byte>(Unsafe.Add<byte>(array.Data, row * Component.GetInfo(cid).SizeInBytes), Component.GetInfo(cid).SizeInBytes);
    }

    public void SetCount(int newCount)
    {
        count = newCount;
    }

    public bool HasComponent<C>()
        where C : unmanaged, IComponent
    {
        return components.Has(Component.GetInfo<C>().ID);
    }

    public bool HasComponent(ComponentID cid)
    {
        return components.Has(cid);
    }
}