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
    return;
}

switch (args[0])
{
    case "shapes":
        ShapesExample.Run();
        break;
    case "ecs":
        new ECSExample().Run();
        break;
    case "input":
        new InputExample().Run();
        break;
    case "audio":
        new AudioExample().Run();
        break;
    case "imgui":
        new ImGuiExample().Run();
        break;
    case "breakout":
        new BreakoutGame().Run();
        break;
    case "draw-triangle":
        new DrawTriangle().Run();
        break;
    default:
        Console.WriteLine("Unknown example");
        break;
}
