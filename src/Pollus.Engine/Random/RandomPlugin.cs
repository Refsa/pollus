namespace Pollus.Engine;

using Pollus.ECS;
using Pollus.ECS.Core;
using Pollus.Mathematics;
using Pollus.Utils;

public class RandomContainer
{
    Random[] sources = new Random[Environment.ProcessorCount];

    public RandomContainer(int? seed)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            var source = seed switch
            {
                null => new System.Random(),
                not null => new System.Random(seed.Value + i),
            };
            sources[i] = new Random(source);
        }
    }

    public Random GetRandom()
    {
        return sources[Environment.CurrentManagedThreadId % sources.Length];
    }
}

public class Random
{
    System.Random source;
    public System.Random Source => source;

    public Random(System.Random source)
    {
        this.source = source;
    }

    public float NextFloat() => Source.NextSingle();
    public Vec2f NextVec2f() => new(NextFloat() * 2f - 1f, NextFloat() * 2f - 1f);
    public Vec3f NextVec3f() => new(NextFloat(), NextFloat(), NextFloat());
    public Vec4f NextVec4f() => new(NextFloat(), NextFloat(), NextFloat(), NextFloat());
    public int NextInt() => Source.Next();
    public Vec2<int> NextInt2() => new(NextInt(), NextInt());
    public Vec3<int> NextInt3() => new(NextInt(), NextInt(), NextInt());
    public Vec4<int> NextInt4() => new(NextInt(), NextInt(), NextInt(), NextInt());
    public uint NextUInt() => Hashes.ToUInt(Source.Next());
}

public class RandomFetch : IFetch<Random>
{
    public static void Register()
    {
        Fetch.Register(new RandomFetch(), []);
    }

    public Random DoFetch(World world, ISystem system)
    {
        return world.Resources.Get<RandomContainer>().GetRandom();
    }
}

public class RandomPlugin : IPlugin
{
    static RandomPlugin()
    {
        RandomFetch.Register();
    }

    public int? Seed { get; set; }

    public void Apply(World world)
    {
        world.Resources.Add(new RandomContainer(Seed));
    }
}