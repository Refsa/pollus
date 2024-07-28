namespace Pollus.ECS;

public record struct Entity(int ID)
{
    public static readonly Entity NULL = new Entity(-1);
}