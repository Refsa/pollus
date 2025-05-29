using Pollus.Examples;
using Pollus.Examples.Flocking;

if (args.Length < 1)
{
    Console.WriteLine("Usage: <example>");
    Console.WriteLine("Examples:");
    Console.WriteLine("  shapes");
    Console.WriteLine("  ecs");
    Console.WriteLine("  input");
    Console.WriteLine("  audio");
    Console.WriteLine("  imgui");
    Console.WriteLine("  breakout");
    Console.WriteLine("  draw-triangle");
    Console.WriteLine("  collision");
    Console.WriteLine("  frame-graph");
    Console.WriteLine("  sprite-benchmark");
    Console.WriteLine("  compute");
    Console.WriteLine("  mesh-rendering");
    Console.WriteLine("  coroutine");
    Console.WriteLine("  change-tracking");
    Console.WriteLine("  ecs-spawn");
    Console.WriteLine("  hierarchy");
    Console.WriteLine("  transform");
    Console.WriteLine("  flocking");
    Console.WriteLine("  gizmo");
    Console.WriteLine("  tween");
    Console.WriteLine("  ecs-iter");
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
    Console.WriteLine("Unknown example: " + args[0]);
    return;
}

example?.Run();