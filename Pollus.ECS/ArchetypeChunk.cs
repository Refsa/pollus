namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public struct ArchetypeChunk : IDisposable
{
    NativeMap<ComponentID, NativeArray<byte>> components;

    int count;
    int length;

    public int Count => count;
    public int Length => length;

    public ArchetypeChunk(Span<ComponentID> cids, int rows = 0)
    {
        length = rows;
        count = 0;

        foreach (var cid in cids)
        {
            var cinfo = Component.GetInfo(cid);
            components.Add(cid, new(rows * cinfo.SizeInBytes));
        }
    }

    public int AddEntity()
    {
        if (count >= length) return -1;
        return count++;
    }

    public void RemoveEntity()
    {
        count--;
    }

    unsafe public void MoveEntity(int row, ref ArchetypeChunk destination)
    {
        if (destination.count >= destination.length) return;

        for (int i = 0; i < components.Count; i++)
        {
            var cid = components.Keys[i];
            var cinfo = Component.GetInfo(cid);
            var srcArray = components.Values[i];
            var dstArray = destination.components.Get(cid);

            Unsafe.CopyBlock(Unsafe.Add<byte>(dstArray.Data, destination.count * cinfo.SizeInBytes),
                             Unsafe.Add<byte>(srcArray.Data, row * cinfo.SizeInBytes),
                             (uint)cinfo.SizeInBytes);
        }

        destination.count++;
    }

    unsafe public void SwapMoveEntity(int row, ref ArchetypeChunk source)
    {
        for (int i = 0; i < components.Count; i++)
        {
            var cid = components.Keys[i];
            if (source.HasComponent(cid) is false) continue;

            var cinfo = Component.GetInfo(cid);
            var srcArray = source.components.Get(cid);
            var dstArray = components.Get(cid);

            Unsafe.CopyBlock(Unsafe.Add<byte>(dstArray.Data, count * cinfo.SizeInBytes),
                             Unsafe.Add<byte>(srcArray.Data, row * cinfo.SizeInBytes),
                             (uint)cinfo.SizeInBytes);
        }

        count++;
        source.count--;
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
        var cinfo = Component.GetInfo<C>();
        ref var array = ref components.Get(cinfo.ID);
        Unsafe.Write(Unsafe.Add<C>(array.Data, row), component);
    }

    unsafe public ref C GetComponent<C>(int row)
        where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        var array = components.Get(cid);
        return ref *(C*)Unsafe.Add<C>(array.Data, row);
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

    public void Dispose()
    {
        foreach (var value in components.Values)
        {
            value.Dispose();
        }
        components.Dispose();
    }
}