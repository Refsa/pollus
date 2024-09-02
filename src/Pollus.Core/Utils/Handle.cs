namespace Pollus.Utils;

public record struct Handle(int Type, int ID)
{
    public static Handle Null => new(-1, -1);

    readonly int hashCode = HashCode.Combine(Type, ID);
    public override int GetHashCode() => hashCode;
}
