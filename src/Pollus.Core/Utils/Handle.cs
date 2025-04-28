namespace Pollus.Utils;

public record struct Handle(int Type, int ID)
{
    public static Handle Null => new(-1, -1);

    readonly int hashCode = HashCode.Combine(Type, ID);
    public override int GetHashCode() => hashCode;
}

public record struct Handle<T>(int ID)
{
    static readonly int typeId = TypeLookup.ID<T>();
    public static Handle<T> Null => new(-1);

    public static implicit operator Handle(Handle<T> handle) => new(typeId, handle.ID);
    public static implicit operator Handle<T>(Handle handle) => new(handle.ID);

    public override int GetHashCode()
    {
        return HashCode.Combine(typeId, ID);
    }
}
