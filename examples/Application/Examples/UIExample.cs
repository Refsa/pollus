namespace Pollus.Examples;

using Engine.Debug;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Rendering;
using Pollus.Engine.UI;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using Pollus.Graphics.Rendering;

public class UIExample : IExample
{
    public string Name => "ui";
    IApplication? app;

    public void Stop() => app?.Shutdown();

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new UIPlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystems(CoreStage.PostInit, FnSystem.Create("UIRectSetup",
            static (Commands commands, AssetServer assetServer, IWindow window) =>
            {
                // Camera is needed by RenderingPlugin's SceneUniform
                commands.Spawn(Camera2D.Bundle);

                var fontHandle = assetServer.LoadAsync<FontAsset>("fonts/SpaceMono-Regular.ttf");

                var viewportW = (float)window.Size.X;
                var viewportH = (float)window.Size.Y;

                // Root container - dark background, fills viewport
                var root = UI.Root(commands, viewportW, viewportH)
                    .AutoResize()
                    .FlexColumn()
                    .Padding(16)
                    .Gap(16)
                    .Background(new Color(0.12f, 0.12f, 0.15f, 1f))
                    .Spawn();

                // Header bar — uses .Style() lambda for consolidated layout
                var header = UI.Panel(commands)
                    .Style(s => s with
                    {
                        Size = new(Length.Auto, 60),
                        Padding = Rect<Length>.All(20),
                        AlignItems = AlignItems.Center,
                    })
                    .Background(new Color(0.2f, 0.4f, 0.8f, 1f))
                    .BorderRadius(8)
                    .Children(
                        UI.Text(commands, "UI Demo", fontHandle).FontSize(20f).Spawn()
                    )
                    .Spawn();

                // Content row: sidebar + main
                var contentRow = UI.Panel(commands)
                    .FlexGrow(1f).FlexRow().Gap(16)
                    .Spawn();

                // Sidebar with interactive buttons — uses .Style() lambda
                var sidebar = UI.Panel(commands)
                    .Style(s => s with
                    {
                        Size = new(200, Length.Auto),
                        FlexDirection = FlexDirection.Column,
                        Padding = Rect<Length>.All(12),
                        Gap = new(8, 8),
                    })
                    .Background(new Color(0.18f, 0.18f, 0.22f, 1f))
                    .BorderRadius(8)
                    .Spawn();

                // Sidebar buttons
                string[] buttonLabels = ["Dashboard", "Settings", "Profile"];
                for (int i = 0; i < buttonLabels.Length; i++)
                {
                    _ = UI.Button(commands)
                        .Height(40).Padding(8, 12, 8, 12)
                        .AlignItems(AlignItems.Center)
                        .Colors(
                            new Color(0.25f, 0.25f, 0.30f, 1f),
                            new Color(0.30f, 0.35f, 0.50f, 1f),
                            new Color(0.20f, 0.25f, 0.45f, 1f),
                            new Color(0.20f, 0.20f, 0.22f, 0.5f))
                        .Background(new Color(0.25f, 0.25f, 0.30f, 1f))
                        .Focusable()
                        .BorderRadius(6)
                        .ChildOf(sidebar)
                        .Children(
                            UI.Text(commands, buttonLabels[i], fontHandle).FontSize(14f).Spawn()
                        )
                        .Spawn();
                }

                // Toggle in sidebar
                _ = UI.Toggle(commands)
                    .IsOn(false)
                    .OnColor(new Color(0.2f, 0.7f, 0.3f, 1f))
                    .OffColor(new Color(0.4f, 0.2f, 0.2f, 1f))
                    .Height(40).Padding(8, 12, 8, 12)
                    .AlignItems(AlignItems.Center)
                    .Background(new Color(0.4f, 0.2f, 0.2f, 1f))
                    .Focusable()
                    .BorderRadius(6)
                    .ChildOf(sidebar)
                    .Children(
                        UI.Text(commands, "Toggle", fontHandle).FontSize(14f).Spawn()
                    )
                    .Spawn();

                // Main panel with border - column layout for sections (scrollable)
                var mainPanel = UI.Panel(commands)
                    .FlexGrow(1f).FlexColumn().Padding(16).Gap(16)
                    .Border(2)
                    .Overflow(Overflow.Scroll, Overflow.Scroll)
                    .Background(new Color(0.15f, 0.15f, 0.19f, 1f))
                    .BorderColor(new Color(0.3f, 0.3f, 0.4f, 1f))
                    .BorderRadius(12)
                    .Interactable()
                    .Spawn();

                // --- Cards row ---
                var cardsRow = UI.Panel(commands)
                    .FlexRow().FlexWrap().Gap(12)
                    .ChildOf(mainPanel)
                    .Spawn();

                Color[] cardColors =
                [
                    new(0.9f, 0.3f, 0.3f, 1f),
                    new(0.3f, 0.8f, 0.4f, 1f),
                    new(0.3f, 0.5f, 0.9f, 1f),
                ];
                string[] cardLabels = ["Card A", "Card B", "Card C"];
                float[] cardRadii = [4, 12, 24];

                for (int i = 0; i < 3; i++)
                {
                    var baseColor = new Color(0.2f, 0.2f, 0.25f, 1f);
                    var card = UI.Button(commands)
                        .Size(160, 120)
                        .FlexColumn().Padding(12).Border(2)
                        .Colors(
                            baseColor,
                            new Color(
                                baseColor.R * 0.7f + cardColors[i].R * 0.3f,
                                baseColor.G * 0.7f + cardColors[i].G * 0.3f,
                                baseColor.B * 0.7f + cardColors[i].B * 0.3f,
                                1f),
                            new Color(
                                cardColors[i].R * 0.6f,
                                cardColors[i].G * 0.6f,
                                cardColors[i].B * 0.6f,
                                1f),
                            new Color(0.15f, 0.15f, 0.15f, 0.5f))
                        .Background(baseColor)
                        .Focusable()
                        .BorderColor(cardColors[i])
                        .BorderRadius(cardRadii[i])
                        .ChildOf(cardsRow)
                        .Spawn();

                    UI.Text(commands, cardLabels[i], fontHandle).FontSize(16f).Color(cardColors[i]).ChildOf(card).Spawn();

                    UI.Text(commands, "This is a card with text that may overflow the container bounds.", fontHandle)
                        .FontSize(12f).Color(new Color(0.7f, 0.7f, 0.7f, 1f))
                        .Margin(8, 0, 0, 0)
                        .ChildOf(card)
                        .Spawn();
                }

                // --- CheckBox section ---
                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Checkboxes", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        UI.CheckBoxGroup(commands, fontHandle)
                            .FlexColumn().Gap(6)
                            .Checked(1)
                            .Option("Enable notifications")
                            .Option("Dark mode")
                            .Option("Auto-save")
                            .Spawn()
                    )
                    .Spawn();

                // --- RadioButton section ---
                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Priority", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        UI.RadioGroup(commands, fontHandle)
                            .FlexColumn().Gap(6)
                            .Option("Low")
                            .Option("Medium")
                            .Option("High")
                            .Selected(1)
                            .Spawn()
                    )
                    .Spawn();

                // --- Slider section ---
                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Volume", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        UI.Slider(commands)
                            .Value(60).Range(0, 100).Step(5)
                            .Size(300, 16)
                            .Background(new Color(0.3f, 0.3f, 0.3f, 1f))
                            .Focusable()
                            .BorderRadius(8)
                            .Spawn()
                    )
                    .Spawn();

                // --- TextInput section ---
                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Text Input", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        UI.TextInput(commands, fontHandle)
                            .FontSize(14f)
                            .Size(300, 32)
                            .Padding(6, 8, 6, 8)
                            .AlignItems(AlignItems.Center)
                            .Border(1)
                            .Background(new Color(0.22f, 0.22f, 0.27f, 1f))
                            .BorderColor(new Color(0.4f, 0.4f, 0.5f, 1f))
                            .BorderRadius(4)
                            .Spawn()
                    )
                    .Spawn();

                // --- NumberInput section ---
                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Quantity (0-99)", fontHandle)
                            .FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        UI.NumberInput(commands, fontHandle)
                            .Range(0, 99).Step(1).Type(NumberInputType.Int)
                            .FontSize(14f)
                            .Size(120, 32)
                            .Padding(6, 8, 6, 8)
                            .AlignItems(AlignItems.Center)
                            .Border(1)
                            .Background(new Color(0.22f, 0.22f, 0.27f, 1f))
                            .BorderColor(new Color(0.4f, 0.4f, 0.5f, 1f))
                            .BorderRadius(4)
                            .Spawn()
                    )
                    .Spawn();


                // --- Dropdown section ---
                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Theme", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        UI.Dropdown(commands, fontHandle)
                            .Placeholder("Dark")
                            .Options("Dark", "Light", "Solarized", "Nord")
                            .FontSize(12f)
                            .Size(200, 32)
                            .Padding(6, 10, 6, 10)
                            .AlignItems(AlignItems.Center)
                            .Border(1)
                            .Background(new Color(0.25f, 0.25f, 0.30f, 1f))
                            .BorderColor(new Color(0.4f, 0.4f, 0.5f, 1f))
                            .BorderRadius(4)
                            .Spawn()
                    )
                    .Spawn();

                // --- SDF Shapes demo ---
                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "SDF Shapes", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        UI.Panel(commands)
                            .FlexRow().Gap(16).AlignItems(AlignItems.Center)
                            .Children(
                                UI.Panel(commands)
                                    .Size(48, 48)
                                    .Background(new Color(0.9f, 0.3f, 0.3f, 1f))
                                    .BorderRadius(8)
                                    .Shape(UIShapeType.RoundedRect)
                                    .Spawn(),
                                UI.Panel(commands)
                                    .Size(48, 48)
                                    .Background(new Color(0.3f, 0.8f, 0.4f, 1f))
                                    .Shape(UIShapeType.Circle)
                                    .Spawn(),
                                UI.Panel(commands)
                                    .Size(48, 48)
                                    .Background(new Color(0.3f, 0.5f, 0.9f, 1f))
                                    .Shape(UIShapeType.Checkmark)
                                    .Spawn(),
                                UI.Panel(commands)
                                    .Size(48, 48)
                                    .Background(new Color(0.9f, 0.7f, 0.2f, 1f))
                                    .Shape(UIShapeType.DownArrow)
                                    .Spawn()
                            )
                            .Spawn()
                    )
                    .Spawn();

                // --- Custom Material demo ---
                {
                    var customMaterial = assetServer.GetAssets<UIRectMaterial>().Add(new UIRectMaterial
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/ui_rect_custom.wgsl"),
                        Texture = assetServer.GetAssets<Texture2D>().Add(new Texture2D
                        {
                            Name = "custom_white_pixel",
                            Width = 1,
                            Height = 1,
                            Format = TextureFormat.Rgba8Unorm,
                            Data = [255, 255, 255, 255],
                        }),
                        Sampler = assetServer.Load<SamplerAsset>("internal://samplers/nearest"),
                    });

                    var defaultPanel = UI.Panel(commands)
                        .Size(160, 100).FlexColumn().Padding(12)
                        .Background(new Color(0.3f, 0.6f, 0.9f, 1f))
                        .BorderRadius(12)
                        .Children(
                            UI.Text(commands, "Default", fontHandle).FontSize(14f).Spawn()
                        )
                        .Spawn();

                    var customPanel = UI.Panel(commands)
                        .Size(160, 100).FlexColumn().Padding(12)
                        .Background(new Color(0.3f, 0.6f, 0.9f, 1f))
                        .BorderRadius(12)
                        .Material(customMaterial)
                        .Children(
                            UI.Text(commands, "Custom", fontHandle).FontSize(14f).Spawn()
                        )
                        .Spawn();

                    var customCircle = UI.Panel(commands)
                        .Size(80, 80)
                        .Background(new Color(0.9f, 0.4f, 0.3f, 1f))
                        .Shape(UIShapeType.Circle)
                        .Material(customMaterial)
                        .Spawn();

                    _ = UI.Panel(commands)
                        .FlexColumn().Gap(8)
                        .ChildOf(mainPanel)
                        .Children(
                            UI.Text(commands, "Custom Material", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                            UI.Panel(commands)
                                .FlexRow().Gap(16).AlignItems(AlignItems.Center)
                                .Children(defaultPanel, customPanel, customCircle)
                                .Spawn()
                        )
                        .Spawn();
                }

                // --- UI Images demo ---
                var testTexture = assetServer.LoadAsync<Texture2D>("textures/test.png");

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Images", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        UI.Panel(commands)
                            .FlexRow().Gap(16).AlignItems(AlignItems.Center).FlexWrap()
                            .Children(
                                // Plain image
                                UI.Image(commands, testTexture)
                                    .Size(96, 96)
                                    .Spawn(),
                                // Image with border radius
                                UI.Image(commands, testTexture)
                                    .Size(96, 96)
                                    .BorderRadius(16)
                                    .Spawn(),
                                // Image with color tint
                                UI.Image(commands, testTexture)
                                    .Size(96, 96)
                                    .Background(new Color(0.4f, 0.7f, 1f, 1f))
                                    .BorderRadius(8)
                                    .Spawn(),
                                // Image with UV slice (top-left quadrant)
                                UI.Image(commands, testTexture)
                                    .Size(96, 96)
                                    .Slice(new Rect(Vec2f.Zero, new Vec2f(0.5f, 0.5f)))
                                    .Border(2)
                                    .BorderColor(new Color(0.5f, 0.8f, 1f, 1f))
                                    .BorderRadius(8)
                                    .Spawn()
                            )
                            .Spawn()
                    )
                    .Spawn();

                // Build hierarchy
                commands.Entity(root).AddChild(header);
                commands.Entity(root).AddChild(contentRow);
                commands.Entity(contentRow).AddChild(sidebar);
                commands.Entity(contentRow).AddChild(mainPanel);
            }))
        .Build()).Run();
}
