namespace Pollus.Utils;

/// <summary>
/// Thread-unsafe object pool
/// </summary>
/// <typeparam name="T"></typeparam>
public class Pool<T>
    where T : new()
{
    static Pool<T>? shared;
    public static Pool<T> Shared => shared ??= new Pool<T>(static () => new(), 16);

    public delegate T FactoryDelegate();

    readonly FactoryDelegate factory;
    readonly Stack<T> open = [];

    public Pool(FactoryDelegate factory, int capacity)
    {
        this.factory = factory;
        open = new Stack<T>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            open.Push(factory());
        }
    }

    public T Rent()
    {
        if (open.TryPop(out var item)) return item;
        return factory();
    }

    public void Return(T item)
    {
        open.Push(item);
    }
}
