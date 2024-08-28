namespace Pollus.ECS;

public partial record struct Entity(int ID, int Version = 0)
{
    public static readonly Entity NULL = new Entity(0, -1);
}
