using Pollus.Debugging;
using Pollus.Examples;
using Pollus.Examples.Flocking;

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
    _ => null
};

if (example is null)
{
    Log.Info("Unknown example: " + args[0]);
    return;
}

Log.Info("Running example: " + args[0]);
example.Run();