namespace Pollus.ECS;

public partial record struct Entity(int ID)
{
    public static readonly Entity NULL = new Entity(0);
}
