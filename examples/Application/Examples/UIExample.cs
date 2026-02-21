namespace Pollus.Examples;

using System.Text.Unicode;
using Pollus.Assets;
using Pollus.Collections;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Engine.Rendering;
using Pollus.Engine.UI;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

public class UIExample : IExample
{
    public string Name => "ui";
    IApplication? app;

    sealed class UIExampleState
    {
        public Entity EventLogTextEntity = Entity.Null;
        public Entity EventLogViewportEntity = Entity.Null;
        public Entity FocusStatusTextEntity = Entity.Null;
        public Entity ThemeDropdownEntity = Entity.Null;
        public string[] ThemeOptions = [];
        public string LastEventLogText = "";
        public int EventLogAutoScrollFrames = 0;
        public readonly Dictionary<Entity, string> Labels = [];
        public readonly List<string> EventLog = [];

        public const int MaxEventLines = 16;
    }

    static void RegisterLabel(UIExampleState state, Entity entity, string label)
    {
        state.Labels[entity] = label;
    }

    static string GetLabel(UIExampleState state, Entity entity)
    {
        return state.Labels.TryGetValue(entity, out var label) ? label : entity.ToString();
    }

    static void PushEventLog(UIExampleState state, string line)
    {
        state.EventLog.Add(line);
        while (state.EventLog.Count > UIExampleState.MaxEventLines)
            state.EventLog.RemoveAt(0);
    }

    static void DisableInteraction(Commands commands, Entity entity)
    {
        commands.SetComponent(entity, new UIInteraction
        {
            State = InteractionState.Disabled,
            Focusable = false,
        });
    }

    public void Stop() => app?.Shutdown();

    public void Run() => (app = Application.Builder
        .AddResource(new UIExampleState())
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new UIPlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystems(CoreStage.PostInit, FnSystem.Create("UIRectSetup",
            static (Commands commands, AssetServer assetServer, IWindow window, UIExampleState state, UIFocusVisualStyle focusVisualStyle) =>
            {
                // Camera is needed by RenderingPlugin's SceneUniform
                commands.Spawn(Camera2D.Bundle);

                focusVisualStyle.Color = new Color(0.98f, 0.67f, 0.26f, 1f);
                focusVisualStyle.Width = 2f;
                focusVisualStyle.Offset = 1f;
                focusVisualStyle.KeyboardOnly = true;

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

                // Header bar - top level title
                var header = UI.Panel(commands)
                    .Style(s => s with
                    {
                        Size = new(Length.Auto, 60),
                        Padding = Rect<Length>.All(20),
                        AlignItems = AlignItems.Center,
                        JustifyContent = JustifyContent.SpaceBetween,
                    })
                    .Background(new Color(0.2f, 0.4f, 0.8f, 1f))
                    .BorderRadius(8)
                    .Children(
                        UI.Text(commands, "UI Demo Workspace", fontHandle).FontSize(20f).Spawn(),
                        UI.Text(commands, "Main controls on left, live event console on right", fontHandle)
                            .FontSize(12f).Color(new Color(0.88f, 0.92f, 1f, 1f)).Spawn()
                    )
                    .Spawn();

                // Workspace shell: left showcase + right inspector
                var workspace = UI.Panel(commands)
                    .FlexGrow(1f).FlexRow().Gap(16)
                    .Style(s => s with
                    {
                        MinSize = new(Length.Px(0), Length.Px(0)),
                    })
                    .Spawn();

                var showcaseSurface = UI.Panel(commands)
                    .FlexGrow(1f).FlexColumn().Padding(12).Gap(12)
                    .Style(s => s with
                    {
                        MinSize = new(Length.Px(0), Length.Px(0)),
                    })
                    .Background(new Color(0.14f, 0.14f, 0.18f, 1f))
                    .Border(1)
                    .BorderColor(new Color(0.24f, 0.24f, 0.3f, 1f))
                    .BorderRadius(12)
                    .Spawn();

                var inspectorColumn = UI.Panel(commands)
                    .Style(s => s with
                    {
                        Size = new(360, Length.Auto),
                        FlexShrink = 0f,
                        FlexDirection = FlexDirection.Column,
                        Gap = new(0, 0),
                        Overflow = new Point<Overflow>(Overflow.Hidden, Overflow.Hidden),
                        MinSize = new(Length.Px(0), Length.Px(0)),
                    })
                    .Padding(12)
                    .Background(new Color(0.13f, 0.13f, 0.17f, 1f))
                    .Border(1)
                    .BorderColor(new Color(0.24f, 0.24f, 0.3f, 1f))
                    .BorderRadius(12)
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(4)
                    .ChildOf(showcaseSurface)
                    .Children(
                        UI.Text(commands, "Widget Gallery", fontHandle).FontSize(15f).Color(new Color(0.8f, 0.87f, 1f, 1f)).Spawn()
                    )
                    .Spawn();

                // Content row inside showcase: sidebar + main gallery
                var contentRow = UI.Panel(commands)
                    .FlexGrow(1f).FlexRow().Gap(16)
                    .Style(s => s with
                    {
                        MinSize = new(Length.Px(0), Length.Px(0)),
                    })
                    .Spawn();

                // Sidebar with interactive buttons
                var sidebar = UI.Panel(commands)
                    .Style(s => s with
                    {
                        Size = new(210, Length.Auto),
                        FlexDirection = FlexDirection.Column,
                        Padding = Rect<Length>.All(12),
                        Gap = new(8, 8),
                    })
                    .Background(new Color(0.18f, 0.18f, 0.22f, 1f))
                    .BorderRadius(8)
                    .Spawn();

                string[] buttonLabels = ["Dashboard", "Settings", "Profile"];
                for (int i = 0; i < buttonLabels.Length; i++)
                {
                    var sidebarButton = UI.Button(commands)
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

                    RegisterLabel(state, sidebarButton, $"Sidebar/{buttonLabels[i]}");
                }

                var sidebarToggle = UI.Toggle(commands)
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

                RegisterLabel(state, sidebarToggle, "Sidebar/Toggle");

                var mainPanel = UI.Panel(commands)
                    .FlexGrow(1f).FlexColumn().Padding(16).Gap(16)
                    .Style(s => s with
                    {
                        MinSize = new(Length.Px(0), Length.Px(0)),
                    })
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

                    RegisterLabel(state, card, cardLabels[i]);

                    UI.Text(commands, cardLabels[i], fontHandle).FontSize(16f).Color(cardColors[i]).ChildOf(card).Spawn();

                    UI.Text(commands, "This is a card with text that may overflow the container bounds.", fontHandle)
                        .FontSize(12f).Color(new Color(0.7f, 0.7f, 0.7f, 1f))
                        .Margin(8, 0, 0, 0)
                        .ChildOf(card)
                        .Spawn();
                }

                // --- CheckBox section ---
                var checkBoxGroup = UI.CheckBoxGroup(commands, fontHandle)
                    .FlexColumn().Gap(6)
                    .Checked(1)
                    .Option("Enable notifications")
                    .Option("Dark mode")
                    .Option("Auto-save")
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Checkboxes", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        checkBoxGroup
                    )
                    .Spawn();

                // --- RadioButton section ---
                var radioGroup = UI.RadioGroup(commands, fontHandle)
                    .FlexColumn().Gap(6)
                    .Option("Low")
                    .Option("Medium")
                    .Option("High")
                    .Selected(1)
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Priority", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        radioGroup
                    )
                    .Spawn();

                // --- Slider section ---
                var volumeSlider = UI.Slider(commands)
                    .Value(60).Range(0, 100).Step(5)
                    .Size(300, 16)
                    .Background(new Color(0.3f, 0.3f, 0.3f, 1f))
                    .FillColor(new Color(0.3f, 0.55f, 1f, 1f))
                    .Focusable()
                    .BorderRadius(8)
                    .Spawn();

                RegisterLabel(state, volumeSlider.Entity, "Main/Volume slider");

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Volume", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        volumeSlider
                    )
                    .Spawn();

                // --- TextInput section ---
                var mainTextInput = UI.TextInput(commands, fontHandle)
                    .FontSize(14f)
                    .Size(300, 32)
                    .Padding(6, 8, 6, 8)
                    .AlignItems(AlignItems.Center)
                    .Border(1)
                    .Background(new Color(0.22f, 0.22f, 0.27f, 1f))
                    .BorderColor(new Color(0.4f, 0.4f, 0.5f, 1f))
                    .BorderRadius(4)
                    .Spawn();

                RegisterLabel(state, mainTextInput.Entity, "Main/Text input");

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Text Input", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        mainTextInput
                    )
                    .Spawn();

                // --- NumberInput section ---
                var quantityInput = UI.NumberInput(commands, fontHandle)
                    .Range(0, 99).Step(1).Type(NumberInputType.Int)
                    .FontSize(14f)
                    .Size(120, 32)
                    .Padding(6, 8, 6, 8)
                    .AlignItems(AlignItems.Center)
                    .Border(1)
                    .Background(new Color(0.22f, 0.22f, 0.27f, 1f))
                    .BorderColor(new Color(0.4f, 0.4f, 0.5f, 1f))
                    .BorderRadius(4)
                    .Spawn();

                RegisterLabel(state, quantityInput.Entity, "Main/Quantity input");

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Quantity (0-99)", fontHandle)
                            .FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        quantityInput
                    )
                    .Spawn();

                // --- Dropdown section ---
                string[] themeOptions = ["Dark", "Light", "Solarized", "Nord"];
                var themeDropdown = UI.Dropdown(commands, fontHandle)
                    .Placeholder("Dark")
                    .Options(themeOptions)
                    .FontSize(12f)
                    .Size(200, 32)
                    .Padding(6, 10, 6, 10)
                    .AlignItems(AlignItems.Center)
                    .Border(1)
                    .Background(new Color(0.25f, 0.25f, 0.30f, 1f))
                    .BorderColor(new Color(0.4f, 0.4f, 0.5f, 1f))
                    .BorderRadius(4)
                    .Spawn();

                state.ThemeDropdownEntity = themeDropdown.Entity;
                state.ThemeOptions = themeOptions;
                RegisterLabel(state, themeDropdown.Entity, "Main/Theme dropdown");
                for (int i = 0; i < themeDropdown.OptionEntities.Length; i++)
                {
                    RegisterLabel(state, themeDropdown.OptionEntities[i], $"Theme option/{themeOptions[i]}");
                }

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Theme", fontHandle).FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                        themeDropdown
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

                // --- custom material ---
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
                        ExtraBindGroups = [
                            [
                                TextureBinding.From(assetServer.LoadAsync<Texture2D>("textures/perlin.png")),
                                SamplerBinding.From(assetServer.Load<SamplerAsset>("internal://samplers/linear")),
                            ],
                        ],
                    });

                    var custom1 = UI.Panel(commands)
                        .Size(200, 120).FlexColumn().Padding(12)
                        .Background(new Color(0.2f, 0.4f, 0.7f, 1f))
                        .BorderRadius(12)
                        .Material(customMaterial)
                        .Children(
                            UI.Text(commands, "Custom Material", fontHandle).FontSize(14f).Spawn()
                        )
                        .Spawn();

                    var custom2 = UI.Panel(commands)
                        .Size(100, 100)
                        .Background(new Color(0.7f, 0.3f, 0.5f, 1f))
                        .Shape(UIShapeType.Circle)
                        .Material(customMaterial)
                        .Spawn();

                    var custom3 = UI.Panel(commands)
                        .Size(160, 70)
                        .Background(new Color(0.3f, 0.7f, 0.4f, 1f))
                        .BorderRadius(35)
                        .Material(customMaterial)
                        .Spawn();

                    _ = UI.Panel(commands)
                        .FlexColumn().Gap(8)
                        .ChildOf(mainPanel)
                        .Children(
                            UI.Text(commands, "Custom Material", fontHandle)
                                .FontSize(16f).Color(new Color(0.5f, 0.8f, 1f, 1f)).Spawn(),
                            UI.Panel(commands)
                                .FlexRow().Gap(16).AlignItems(AlignItems.Center)
                                .Children(custom1, custom2, custom3)
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
                                UI.Image(commands, testTexture)
                                    .Size(96, 96)
                                    .Spawn(),
                                UI.Image(commands, testTexture)
                                    .Size(96, 96)
                                    .BorderRadius(16)
                                    .Spawn(),
                                UI.Image(commands, testTexture)
                                    .Size(96, 96)
                                    .Background(new Color(0.4f, 0.7f, 1f, 1f))
                                    .BorderRadius(8)
                                    .Spawn(),
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

                // 1) Event Console
                state.EventLog.Clear();
                PushEventLog(state, "[system] Events will be shown here");

                var initialEventLog = string.Join("\n", state.EventLog);
                state.LastEventLogText = initialEventLog;

                var eventLogText = UI.Text(commands, initialEventLog, fontHandle)
                    .FontSize(12f)
                    .Color(new Color(0.78f, 0.85f, 0.95f, 1f))
                    .Style(s => s with
                    {
                        Size = new(Length.Percent(1f), Length.Auto),
                    })
                    .Spawn();

                state.EventLogTextEntity = eventLogText;

                var eventLogViewport = UI.Panel(commands)
                    .FlexGrow(1f)
                    .Style(s => s with
                    {
                        MinSize = new(Length.Px(0), Length.Px(0)),
                    })
                    .Padding(10)
                    .Overflow(Overflow.Hidden, Overflow.Scroll)
                    .Background(new Color(0.11f, 0.11f, 0.15f, 1f))
                    .Border(1)
                    .BorderColor(new Color(0.22f, 0.22f, 0.29f, 1f))
                    .BorderRadius(8)
                    .Children(eventLogText)
                    .Spawn();
                state.EventLogViewportEntity = eventLogViewport;
                state.EventLogAutoScrollFrames = 3;

                _ = UI.Panel(commands)
                    .FlexGrow(1f).FlexColumn().Gap(8)
                    .Style(s => s with
                    {
                        MinSize = new(Length.Px(0), Length.Px(0)),
                    })
                    .Padding(12)
                    .Background(new Color(0.16f, 0.16f, 0.21f, 1f))
                    .Border(1)
                    .BorderColor(new Color(0.25f, 0.25f, 0.32f, 1f))
                    .BorderRadius(10)
                    .ChildOf(inspectorColumn)
                    .Children(
                        UI.Text(commands, "Event Console", fontHandle)
                            .FontSize(16f).Color(new Color(0.55f, 0.85f, 1f, 1f)).Spawn(),
                        UI.Text(commands, "Live stream of click, toggle, slider, input and dropdown events.", fontHandle)
                            .FontSize(12f).Color(new Color(0.68f, 0.73f, 0.82f, 1f)).Spawn(),
                        eventLogViewport
                    )
                    .Spawn();

                // 2) Keyboard + Focus
                var initialFocusStatus = "Focused: none\nSource: -";
                var focusStatusText = UI.Text(commands, initialFocusStatus, fontHandle)
                    .FontSize(12f)
                    .Color(new Color(0.78f, 0.85f, 0.95f, 1f))
                    .Style(s => s with
                    {
                        Size = new(Length.Percent(1f), Length.Auto),
                    })
                    .Spawn();

                state.FocusStatusTextEntity = focusStatusText;

                var defaultFocusButton = UI.Button(commands)
                    .Height(34).Padding(6, 10, 6, 10)
                    .AlignItems(AlignItems.Center)
                    .Colors(
                        new Color(0.27f, 0.27f, 0.33f, 1f),
                        new Color(0.34f, 0.38f, 0.52f, 1f),
                        new Color(0.24f, 0.28f, 0.48f, 1f),
                        new Color(0.2f, 0.2f, 0.2f, 0.5f))
                    .Background(new Color(0.27f, 0.27f, 0.33f, 1f))
                    .Focusable()
                    .BorderRadius(6)
                    .Children(
                        UI.Text(commands, "Default ring", fontHandle).FontSize(12f).Spawn()
                    )
                    .Spawn();
                RegisterLabel(state, defaultFocusButton, "Focus/Default ring button");

                var customFocusButton = UI.Button(commands)
                    .Height(34).Padding(6, 10, 6, 10)
                    .AlignItems(AlignItems.Center)
                    .Colors(
                        new Color(0.27f, 0.27f, 0.33f, 1f),
                        new Color(0.4f, 0.35f, 0.25f, 1f),
                        new Color(0.5f, 0.35f, 0.22f, 1f),
                        new Color(0.2f, 0.2f, 0.2f, 0.5f))
                    .Background(new Color(0.27f, 0.27f, 0.33f, 1f))
                    .Focusable()
                    .BorderRadius(6)
                    .Children(
                        UI.Text(commands, "Custom ring", fontHandle).FontSize(12f).Spawn()
                    )
                    .Spawn();
                commands.AddComponent(customFocusButton, new UIFocusVisual
                {
                    Color = new Color(0.98f, 0.45f, 0.24f, 1f),
                    Width = 3f,
                    Offset = 2f,
                });
                RegisterLabel(state, customFocusButton, "Focus/Custom ring button");

                var noFocusRingButton = UI.Button(commands)
                    .Height(34).Padding(6, 10, 6, 10)
                    .AlignItems(AlignItems.Center)
                    .Colors(
                        new Color(0.27f, 0.27f, 0.33f, 1f),
                        new Color(0.31f, 0.31f, 0.41f, 1f),
                        new Color(0.23f, 0.23f, 0.3f, 1f),
                        new Color(0.2f, 0.2f, 0.2f, 0.5f))
                    .Background(new Color(0.27f, 0.27f, 0.33f, 1f))
                    .Focusable()
                    .NoFocusVisual()
                    .BorderRadius(6)
                    .Children(
                        UI.Text(commands, "No ring", fontHandle).FontSize(12f).Spawn()
                    )
                    .Spawn();
                RegisterLabel(state, noFocusRingButton, "Focus/No ring button");

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .Padding(12)
                    .Background(new Color(0.16f, 0.16f, 0.21f, 1f))
                    .Border(1)
                    .BorderColor(new Color(0.25f, 0.25f, 0.32f, 1f))
                    .BorderRadius(10)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Keyboard + Focus", fontHandle)
                            .FontSize(16f).Color(new Color(0.55f, 0.85f, 1f, 1f)).Spawn(),
                        UI.Text(commands, "Tab/Shift+Tab to move focus. Enter/Space activates. Esc closes Theme dropdown.", fontHandle)
                            .FontSize(12f).Color(new Color(0.68f, 0.73f, 0.82f, 1f)).Spawn(),
                        UI.Panel(commands)
                            .Padding(8)
                            .Background(new Color(0.11f, 0.11f, 0.15f, 1f))
                            .Border(1)
                            .BorderColor(new Color(0.22f, 0.22f, 0.29f, 1f))
                            .BorderRadius(8)
                            .Children(focusStatusText)
                            .Spawn(),
                        UI.Panel(commands)
                            .FlexColumn().Gap(8)
                            .Children(defaultFocusButton, customFocusButton, noFocusRingButton)
                            .Spawn()
                    )
                    .Spawn();

                // 3) Disabled states
                var disabledButton = UI.Button(commands)
                    .Height(34).Padding(6, 10, 6, 10)
                    .AlignItems(AlignItems.Center)
                    .Colors(
                        new Color(0.29f, 0.29f, 0.33f, 1f),
                        new Color(0.29f, 0.29f, 0.33f, 1f),
                        new Color(0.29f, 0.29f, 0.33f, 1f),
                        new Color(0.21f, 0.21f, 0.24f, 0.55f))
                    .Background(new Color(0.29f, 0.29f, 0.33f, 1f))
                    .Focusable()
                    .BorderRadius(6)
                    .Children(
                        UI.Text(commands, "Action", fontHandle).FontSize(12f).Color(new Color(0.72f, 0.72f, 0.75f, 1f)).Spawn()
                    )
                    .Spawn();
                DisableInteraction(commands, disabledButton);
                RegisterLabel(state, disabledButton, "Disabled/Button");

                var disabledToggle = UI.Toggle(commands)
                    .IsOn(false)
                    .OnColor(new Color(0.35f, 0.35f, 0.35f, 1f))
                    .OffColor(new Color(0.27f, 0.27f, 0.27f, 1f))
                    .Height(34).Padding(6, 10, 6, 10)
                    .AlignItems(AlignItems.Center)
                    .Background(new Color(0.27f, 0.27f, 0.27f, 1f))
                    .BorderRadius(6)
                    .Children(
                        UI.Text(commands, "Toggle", fontHandle).FontSize(12f).Color(new Color(0.72f, 0.72f, 0.75f, 1f)).Spawn()
                    )
                    .Spawn();
                DisableInteraction(commands, disabledToggle);
                RegisterLabel(state, disabledToggle, "Disabled/Toggle");

                var disabledCheckBox = UI.CheckBox(commands)
                    .Size(18, 18)
                    .CheckedColor(new Color(0.37f, 0.37f, 0.37f, 1f))
                    .UncheckedColor(new Color(0.26f, 0.26f, 0.26f, 1f))
                    .CheckmarkColor(new Color(0.75f, 0.75f, 0.75f, 1f))
                    .Background(new Color(0.26f, 0.26f, 0.26f, 1f))
                    .BorderRadius(4)
                    .Spawn();
                DisableInteraction(commands, disabledCheckBox);
                RegisterLabel(state, disabledCheckBox, "Disabled/CheckBox");

                var disabledRadio = UI.RadioButton(commands, 404)
                    .Size(18, 18)
                    .SelectedColor(new Color(0.37f, 0.37f, 0.37f, 1f))
                    .UnselectedColor(new Color(0.26f, 0.26f, 0.26f, 1f))
                    .Background(new Color(0.26f, 0.26f, 0.26f, 1f))
                    .Shape(UIShapeType.Circle)
                    .BorderRadius(9)
                    .Spawn();
                DisableInteraction(commands, disabledRadio);
                RegisterLabel(state, disabledRadio, "Disabled/Radio");

                var disabledInput = UI.TextInput(commands, fontHandle)
                    .Text("read-only demo")
                    .FontSize(12f)
                    .Size(180, 30)
                    .Padding(6, 8, 6, 8)
                    .AlignItems(AlignItems.Center)
                    .Border(1)
                    .Background(new Color(0.2f, 0.2f, 0.22f, 1f))
                    .BorderColor(new Color(0.3f, 0.3f, 0.34f, 1f))
                    .BorderRadius(4)
                    .Spawn();
                DisableInteraction(commands, disabledInput.Entity);
                RegisterLabel(state, disabledInput.Entity, "Disabled/Text input");

                var disabledRows = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexRow().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.SpaceBetween)
                    .Style(s => s with { Size = new(Length.Percent(1f), Length.Auto) })
                    .ChildOf(disabledRows)
                    .Children(
                        UI.Text(commands, "Button", fontHandle).FontSize(12f).Color(new Color(0.7f, 0.72f, 0.78f, 1f)).Spawn(),
                        disabledButton
                    )
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexRow().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.SpaceBetween)
                    .Style(s => s with { Size = new(Length.Percent(1f), Length.Auto) })
                    .ChildOf(disabledRows)
                    .Children(
                        UI.Text(commands, "Toggle", fontHandle).FontSize(12f).Color(new Color(0.7f, 0.72f, 0.78f, 1f)).Spawn(),
                        disabledToggle
                    )
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexRow().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.SpaceBetween)
                    .Size(Length.Percent(1f), Length.Auto)
                    .ChildOf(disabledRows)
                    .Children(
                        UI.Text(commands, "CheckBox", fontHandle).FontSize(12f).Color(new Color(0.7f, 0.72f, 0.78f, 1f)).Spawn(),
                        disabledCheckBox
                    )
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexRow().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.SpaceBetween)
                    .Style(s => s with { Size = new(Length.Percent(1f), Length.Auto) })
                    .ChildOf(disabledRows)
                    .Children(
                        UI.Text(commands, "Radio", fontHandle).FontSize(12f).Color(new Color(0.7f, 0.72f, 0.78f, 1f)).Spawn(),
                        disabledRadio
                    )
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexRow().AlignItems(AlignItems.Center).JustifyContent(JustifyContent.SpaceBetween)
                    .Style(s => s with { Size = new(Length.Percent(1f), Length.Auto) })
                    .ChildOf(disabledRows)
                    .Children(
                        UI.Text(commands, "Text Input", fontHandle).FontSize(12f).Color(new Color(0.7f, 0.72f, 0.78f, 1f)).Spawn(),
                        disabledInput
                    )
                    .Spawn();

                _ = UI.Panel(commands)
                    .FlexColumn().Gap(8)
                    .Padding(12)
                    .Background(new Color(0.16f, 0.16f, 0.21f, 1f))
                    .Border(1)
                    .BorderColor(new Color(0.25f, 0.25f, 0.32f, 1f))
                    .BorderRadius(10)
                    .ChildOf(mainPanel)
                    .Children(
                        UI.Text(commands, "Disabled States", fontHandle)
                            .FontSize(16f).Color(new Color(0.55f, 0.85f, 1f, 1f)).Spawn(),
                        disabledRows
                    )
                    .Spawn();

                // Build hierarchy
                commands.Entity(root).AddChild(header);
                commands.Entity(root).AddChild(workspace);
                commands.Entity(workspace).AddChild(showcaseSurface);
                commands.Entity(workspace).AddChild(inspectorColumn);
                commands.Entity(showcaseSurface).AddChild(contentRow);
                commands.Entity(contentRow).AddChild(sidebar);
                commands.Entity(contentRow).AddChild(mainPanel);
            }))
        .AddSystems(CoreStage.PostUpdate, FnSystem.Create(new SystemBuilderDescriptor("UIExample::EventConsole")
        {
            RunsAfter = [
                    "UIInteractionSystem::UpdateState",
                    "UIInteractionSystem::FocusNavigation",
                    "UIToggleSystem::UpdateToggles",
                    "UICheckBoxSystem::UpdateCheckBoxes",
                    "UIRadioButtonSystem::UpdateRadioButtons",
                    "UISliderSystem::PerformUpdate",
                    "UITextInputSystem::PerformTextInput",
                    "UINumberInputSystem::PerformUpdate",
                    "UIDropdownSystem::PerformUpdate",
                ],
        },
            static (
                UIExampleState state,
                UITextBuffers textBuffers,
                View<UIText> viewText,
                EventReader<UIInteractionEvents.UIClickEvent> clickEvents,
                EventReader<UIToggleEvents.UIToggleEvent> toggleEvents,
                EventReader<UICheckBoxEvents.UICheckBoxEvent> checkBoxEvents,
                EventReader<UIRadioButtonEvents.UIRadioButtonEvent> radioEvents,
                EventReader<UISliderEvents.UISliderValueChanged> sliderEvents,
                EventReader<UITextInputEvents.UITextInputValueChanged> textInputEvents,
                EventReader<UINumberInputEvents.UINumberInputValueChanged> numberEvents,
                EventReader<UIDropdownEvents.UIDropdownSelectionChanged> dropdownEvents) =>
            {
                bool changed = false;

                foreach (var ev in clickEvents.Read())
                {
                    PushEventLog(state, $"[click] {GetLabel(state, ev.Entity)}");
                    changed = true;
                }

                foreach (var ev in toggleEvents.Read())
                {
                    PushEventLog(state, $"[toggle] {GetLabel(state, ev.Entity)} -> {(ev.IsOn ? "on" : "off")}");
                    changed = true;
                }

                foreach (var ev in checkBoxEvents.Read())
                {
                    PushEventLog(state, $"[checkbox] {GetLabel(state, ev.Entity)} -> {(ev.IsChecked ? "checked" : "unchecked")}");
                    changed = true;
                }

                foreach (var ev in radioEvents.Read())
                {
                    PushEventLog(state, $"[radio] {GetLabel(state, ev.Entity)} -> selected={ev.IsSelected}");
                    changed = true;
                }

                foreach (var ev in sliderEvents.Read())
                {
                    PushEventLog(state, $"[slider] {GetLabel(state, ev.Entity)} {ev.PreviousValue:0.##} -> {ev.Value:0.##}");
                    changed = true;
                }

                foreach (var ev in textInputEvents.Read())
                {
                    var value = textBuffers.Get(ev.Entity);
                    PushEventLog(state, $"[text] {GetLabel(state, ev.Entity)} = \"{value}\"");
                    changed = true;
                }

                foreach (var ev in numberEvents.Read())
                {
                    PushEventLog(state, $"[number] {GetLabel(state, ev.Entity)} {ev.PreviousValue:0.##} -> {ev.Value:0.##}");
                    changed = true;
                }

                foreach (var ev in dropdownEvents.Read())
                {
                    var selected = ev.SelectedIndex >= 0 && ev.SelectedIndex < state.ThemeOptions.Length
                        ? state.ThemeOptions[ev.SelectedIndex]
                        : $"#{ev.SelectedIndex}";
                    PushEventLog(state, $"[dropdown] {GetLabel(state, ev.Entity)} -> {selected}");
                    changed = true;
                }

                if (!changed || state.EventLogTextEntity.IsNull || !viewText.Has<UIText>(state.EventLogTextEntity))
                    return;

                var logText = string.Join("\n", state.EventLog);
                if (logText == state.LastEventLogText)
                    return;

                state.LastEventLogText = logText;
                state.EventLogAutoScrollFrames = 3;

                ref var eventLogText = ref viewText.GetTracked<UIText>(state.EventLogTextEntity);
                eventLogText.Text = new NativeUtf8(logText);
            }))
        .AddSystems(CoreStage.PostUpdate, FnSystem.Create(new SystemBuilderDescriptor("UIExample::EventLogAutoScroll")
        {
            RunsAfter = [
                    "UIExample::EventConsole",
                    "UILayoutSystem::WriteBack",
                    "UIScrollSystem::UpdateVisuals",
                ],
        },
            static (UIExampleState state, Query<UIScrollOffset, ComputedNode> qScroll) =>
            {
                if (state.EventLogAutoScrollFrames <= 0 || state.EventLogViewportEntity.IsNull)
                    return;

                if (!qScroll.Has<UIScrollOffset>(state.EventLogViewportEntity)
                    || !qScroll.Has<ComputedNode>(state.EventLogViewportEntity))
                    return;

                ref readonly var computed = ref qScroll.Get<ComputedNode>(state.EventLogViewportEntity);
                var innerHeight = computed.Size.Y - computed.PaddingTop - computed.PaddingBottom
                                  - computed.BorderTop - computed.BorderBottom;
                var maxScrollY = MathF.Max(0f, computed.ContentSize.Y - innerHeight);

                ref var scroll = ref qScroll.GetTracked<UIScrollOffset>(state.EventLogViewportEntity);
                scroll.Offset.Y = maxScrollY;
                state.EventLogAutoScrollFrames--;
            }))
        .AddSystems(CoreStage.PostUpdate, FnSystem.Create(new SystemBuilderDescriptor("UIExample::FocusStatus")
        {
            RunsAfter = ["UIInteractionSystem::FocusNavigation"],
        },
            static (UIExampleState state, UIFocusState focusState, View<UIText> viewText) =>
            {
                if (state.FocusStatusTextEntity.IsNull || !viewText.Has<UIText>(state.FocusStatusTextEntity))
                    return;

                var focusedLabel = focusState.FocusedEntity.IsNull
                    ? "none"
                    : GetLabel(state, focusState.FocusedEntity);
                var sourceLabel = focusState.FocusedEntity.IsNull
                    ? "-"
                    : (focusState.FocusSource == FocusSource.Keyboard ? "keyboard" : "mouse");

                Span<byte> buf = stackalloc byte[256];
                if (!Utf8.TryWrite(buf, $"Focused: {focusedLabel}\nSource: {sourceLabel}", out var len))
                    return;

                var newText = buf[..len];
                ref var focusStatusText = ref viewText.GetTracked<UIText>(state.FocusStatusTextEntity);
                if (focusStatusText.Text.ContentEquals(newText))
                    return;

                focusStatusText.Text = new NativeUtf8(newText);
            }))
        .Build()).Run();
}
