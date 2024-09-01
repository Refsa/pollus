using Pollus.Examples;

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
    "draw-triangle" => new DrawTriangle(),
    "collision" => new CollisionExample(),
    _ => null
};

if (example is null)
{
    Console.WriteLine("Unknown example: " + args[0]);
    return;
}

example?.Run();