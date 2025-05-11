namespace Pollus.Utils;

using System.Collections.Concurrent;

public class ConcurrentPool<T>
    where T : new()
{
    static ConcurrentPool<T>? shared;
    public static ConcurrentPool<T> Shared => shared ??= new ConcurrentPool<T>(static () => new(), 16);

    public delegate T FactoryDelegate();

    readonly FactoryDelegate factory;
    readonly ConcurrentQueue<T> open = [];

    public ConcurrentPool(FactoryDelegate factory, int capacity)
    {
        this.factory = factory;
        for (int i = 0; i < capacity; i++)
        {
            open.Enqueue(factory());
        }
    }

    public T Rent()
    {
        if (open.TryDequeue(out var item)) return item;
        return factory();
    }

    public void Return(T item)
    {
        open.Enqueue(item);
    }
}