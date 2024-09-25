namespace Pollus.ECS;

public partial record struct Entity(int ID, int Version = 0)
{
    public static readonly Entity NULL = new Entity(0, -1);

    public bool IsNull => ID <= 0 && Version < 0;
    public override int GetHashCode() => ID;
    public override string ToString() => $"Entity({ID}, {Version})";
}
