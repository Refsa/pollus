namespace Pollus.Examples;

using Debugging;
using Flocking;

public static class ExampleRunner
{
    public static void Run(string[] args)
    {
        var examples = new List<IExample>()
        {
            new ShapesExample(),
            new ECSExample(),
            new InputExample(),
            new AudioExample(),
            new ImGuiExample(),
            new BreakoutGame(),
            new DrawTriangleExample(),
            new CollisionExample(),
            new FrameGraphExample(),
            new SpriteBenchmark(),
            new TransformBenchmark(),
            new ComputeExample(),
            new MeshRenderingExample(),
            new CoroutineExample(),
            new ChangeTrackingExample(),
            new ECSSpawnExample(),
            new HierarchyExample(),
            new TransformExample(),
            new FlockingExample(),
            new GizmoExample(),
            new PerformanceSampling(),
            new TweenExample(),
            new ECSIter(),
            new QueryFilterExample(),
            new FontExample(),
            new SceneExample(),
            new RenderOrderExample(),
            new SpriteMaterialExample(),
            new SpriteAnimationExample(),
            new UIExample(),
            new SystemGroupExample(),
        };

        if (args.Length < 1)
        {
            Log.Info("Usage: <example>");
            Log.Info("Examples:");
            foreach (var ex in examples)
            {
                Log.Info($"  {ex.Name}");
            }
            return;
        }

        IExample? example = examples.Find(ex => ex.Name == args[0]);

        if (example is null)
        {
            Log.Info("Unknown example: " + args[0]);
            return;
        }

        Log.Info("Running example: " + args[0]);
        example.Run();
    }
}
