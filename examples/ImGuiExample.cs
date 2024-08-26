namespace Pollus.Examples;

using Pollus.Engine.Input;
using Pollus.Engine;
using Pollus.Engine.Imgui;
using Pollus.Engine.Rendering;
using Pollus.Engine.Assets;
using Pollus.ECS;
using Pollus.Engine.Camera;

public class ImGuiExample
{
    public void Run() => Application.Builder
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new InputPlugin(),
            new ImguiPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, SystemBuilder.FnSystem("Spawn", (Commands commands) =>
        {
            commands.Spawn(Camera2D.Bundle);
        }))
        .AddSystem(CoreStage.Update, SystemBuilder.FnSystem("ImGui", () =>
        {
            ImGuiNET.ImGui.ShowDemoWindow();
        }))
        .Run();
}