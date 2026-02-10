namespace Pollus.Examples;

using Debugging;
using Flocking;

public static class ExampleRunner
{
    public static void Run(string[] args)
    {
        if (args.Length < 1)
        {
            Log.Info("Usage: <example>");
            Log.Info("Examples:");
            Log.Info("  shapes");
            Log.Info("  ecs");
            Log.Info("  input");
            Log.Info("  audio");
            Log.Info("  imgui");
            Log.Info("  breakout");
            Log.Info("  draw-triangle");
            Log.Info("  collision");
            Log.Info("  frame-graph");
            Log.Info("  sprite-benchmark");
            Log.Info("  compute");
            Log.Info("  mesh-rendering");
            Log.Info("  coroutine");
            Log.Info("  change-tracking");
            Log.Info("  ecs-spawn");
            Log.Info("  hierarchy");
            Log.Info("  transform");
            Log.Info("  flocking");
            Log.Info("  gizmo");
            Log.Info("  tween");
            Log.Info("  ecs-iter");
            Log.Info("  query-filter");
            Log.Info("  font");
            Log.Info("  scene");
            Log.Info("  render-order");
            Log.Info("  sprite-material");
            Log.Info("  sprite-animation");
            Log.Info("  ui-rect");
            return;
        }

        IExample? example = args[0] switch
        {
            "shapes" => new ShapesExample(),
            "ecs" => new ECSExample(),
            "input" => new InputExample(),
            "audio" => new AudioExample(),
            "imgui" => new ImGuiExample(),
            "breakout" => new BreakoutGame(),
            "draw-triangle" => new DrawTriangleExample(),
            "collision" => new CollisionExample(),
            "frame-graph" => new FrameGraphExample(),
            "sprite-benchmark" => new SpriteBenchmark(),
            "compute" => new ComputeExample(),
            "mesh-rendering" => new MeshRenderingExample(),
            "coroutine" => new CoroutineExample(),
            "change-tracking" => new ChangeTrackingExample(),
            "ecs-spawn" => new ECSSpawnExample(),
            "hierarchy" => new HierarchyExample(),
            "transform" => new TransformExample(),
            "flocking" => new FlockingExample(),
            "gizmo" => new GizmoExample(),
            "psample" => new PerformanceSampling(),
            "tween" => new TweenExample(),
            "ecs-iter" => new ECSIter(),
            "query-filter" => new QueryFilterExample(),
            "font" => new FontExample(),
            "scene" => new SceneExample(),
            "render-order" => new RenderOrderExample(),
            "sprite-material" => new SpriteMaterialExample(),
            "sprite-animation" => new SpriteAnimationExample(),
            "ui-rect" => new UIRectExample(),
            _ => null
        };

        if (example is null)
        {
            Log.Info("Unknown example: " + args[0]);
            return;
        }

        Log.Info("Running example: " + args[0]);
        example.Run();
    }
}