namespace Pollus.Utils;

using Debugging;

public record struct Handle(TypeID Type, int ID)
{
    public static Handle Null => new(-1, -1);
    readonly int hashCode = HashCode.Combine(Type, ID);

    public override int GetHashCode() => hashCode;

    public Handle<T> As<T>() where T : notnull
    {
        Guard.IsTrue(Type == Handle<T>.TypeId, (FormattableString)$"Handle::As<T> expected type {Handle<T>.TypeId} but got {Type}");
        return new(ID);
    }
}

public record struct Handle<T>(int ID)
{
    public static readonly TypeID TypeId = TypeLookup.ID<T>();
    public static Handle<T> Null => new(-1);

    public static implicit operator Handle(Handle<T> handle) => new(TypeId, handle.ID);
    public static implicit operator Handle<T>(Handle handle) => new(handle.ID);

    public TypeID TypeID => TypeLookup.ID<T>();

    public override int GetHashCode()
    {
        return HashCode.Combine(TypeId, ID);
    }
}

public static class HandleExtensions
{
    public static bool IsNull<T>(this Handle<T> handle) => handle == Handle<T>.Null;
    public static bool IsNull(this Handle handle) => handle == Handle.Null;
}