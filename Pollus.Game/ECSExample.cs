
namespace Pollus.Game;

using Pollus.ECS;

public class ECSExample
{
    public static void Run()
    {
        using var world = new World();
        for (int i = 0; i < 100_000; i++)
        {
            world.Spawn(new Component1(), new Component2());
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var frameCount = 0;
        while (true)
        {
            var q = new Query<Component1, Component2>(world);
            q.ForEach((ref Component1 c1, ref Component2 c2) =>
            {
                c1.Value++;
                c2.Value++;
            });
            frameCount++;
            if (sw.ElapsedMilliseconds >= 1000)
            {
                Console.WriteLine(frameCount / (sw.ElapsedMilliseconds / 1000.0));
                frameCount = 0;
                sw.Restart();
            }
        }
    }
}