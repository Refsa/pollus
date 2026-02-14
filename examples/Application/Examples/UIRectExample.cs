namespace Pollus.Examples;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using Pollus.Engine.UI;
using LayoutStyle = Pollus.UI.Layout.Style;

public class UIRectExample : IExample
{
    public string Name => "ui-rect";
    IApplication? app;

    public void Stop() => app?.Shutdown();

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new UIPlugin(),
        ])
        .AddSystems(CoreStage.PostInit, FnSystem.Create("UIRectSetup",
            static (Commands commands, Resources resources, AssetServer assetServer, Assets<UIRectMaterial> materials, IWindow window) =>
            {
                // Camera is needed by RenderingPlugin's SceneUniform
                commands.Spawn(Camera2D.Bundle);

                var material = materials.Add(new UIRectMaterial
                {
                    ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/ui_rect.wgsl"),
                });
                resources.Add(new UIRenderResources { Material = material });

                var fontHandle = assetServer.LoadAsync<FontAsset>("fonts/SpaceMono-Regular.ttf");

                var viewportW = (float)window.Size.X;
                var viewportH = (float)window.Size.Y;

                // Root container - dark background, fills viewport
                var root = commands.Spawn(Entity.With(
                    new UINode(),
                    new UILayoutRoot { Size = new Size<float>(viewportW, viewportH) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Percent(1f), Dimension.Percent(1f)),
                            FlexDirection = FlexDirection.Column,
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(16), LengthPercentage.Px(16),
                                LengthPercentage.Px(16), LengthPercentage.Px(16)),
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(16), LengthPercentage.Px(16)),
                        }
                    },
                    new BackgroundColor { Color = new Color(0.12f, 0.12f, 0.15f, 1f) }
                ));

                // Header bar - colored, rounded corners
                var header = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Auto, Dimension.Px(60)),
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(20), LengthPercentage.Px(20),
                                LengthPercentage.Px(20), LengthPercentage.Px(20)),
                            AlignItems = AlignItems.Center,
                        }
                    },
                    new BackgroundColor { Color = new Color(0.2f, 0.4f, 0.8f, 1f) },
                    new BorderRadius { TopLeft = 8, TopRight = 8, BottomLeft = 8, BottomRight = 8 }
                )).Entity;

                // Header text
                var headerText = commands.Spawn(Entity.With(
                    new UIText { Text = new NativeUtf8("UI Demo"), Size = 20f, Color = Color.WHITE },
                    new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                    new UITextFont { Font = fontHandle },
                    new UIStyle { Value = LayoutStyle.Default }
                )).Entity;
                commands.Entity(header).AddChild(headerText);

                // Content row: sidebar + main
                var contentRow = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexGrow = 1f,
                            FlexDirection = FlexDirection.Row,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(16), LengthPercentage.Px(16)),
                        }
                    }
                )).Entity;

                // Sidebar with interactive buttons
                var sidebar = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Auto),
                            FlexGrow = 0f,
                            FlexDirection = FlexDirection.Column,
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(12), LengthPercentage.Px(12),
                                LengthPercentage.Px(12), LengthPercentage.Px(12)),
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                        }
                    },
                    new BackgroundColor { Color = new Color(0.18f, 0.18f, 0.22f, 1f) },
                    new BorderRadius { TopLeft = 8, TopRight = 8, BottomLeft = 8, BottomRight = 8 }
                )).Entity;

                // Sidebar buttons - hover/press to see color change
                string[] buttonLabels = ["Dashboard", "Settings", "Profile"];
                for (int i = 0; i < buttonLabels.Length; i++)
                {
                    var btn = commands.Spawn(Entity.With(
                        new UINode(),
                        new UIInteraction { Focusable = true },
                        new UIButton
                        {
                            NormalColor = new Color(0.25f, 0.25f, 0.30f, 1f),
                            HoverColor = new Color(0.30f, 0.35f, 0.50f, 1f),
                            PressedColor = new Color(0.20f, 0.25f, 0.45f, 1f),
                            DisabledColor = new Color(0.20f, 0.20f, 0.22f, 0.5f),
                        },
                        new BackgroundColor { Color = new Color(0.25f, 0.25f, 0.30f, 1f) },
                        new UIStyle
                        {
                            Value = LayoutStyle.Default with
                            {
                                Size = new Size<Dimension>(Dimension.Auto, Dimension.Px(40)),
                                Padding = new Rect<LengthPercentage>(
                                    LengthPercentage.Px(12), LengthPercentage.Px(12),
                                    LengthPercentage.Px(8), LengthPercentage.Px(8)),
                                AlignItems = AlignItems.Center,
                            }
                        },
                        new BorderRadius { TopLeft = 6, TopRight = 6, BottomLeft = 6, BottomRight = 6 }
                    )).Entity;

                    // Button label text
                    var btnText = commands.Spawn(Entity.With(
                        new UIText { Text = new NativeUtf8(buttonLabels[i]), Size = 14f, Color = Color.WHITE },
                        new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                        new UITextFont { Font = fontHandle },
                        new UIStyle { Value = LayoutStyle.Default }
                    )).Entity;
                    commands.Entity(btn).AddChild(btnText);

                    commands.Entity(sidebar).AddChild(btn);
                }

                // Toggle in sidebar
                var toggle = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIInteraction { Focusable = true },
                    new UIToggle
                    {
                        IsOn = false,
                        OnColor = new Color(0.2f, 0.7f, 0.3f, 1f),
                        OffColor = new Color(0.4f, 0.2f, 0.2f, 1f),
                    },
                    new BackgroundColor { Color = new Color(0.4f, 0.2f, 0.2f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Auto, Dimension.Px(40)),
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(12), LengthPercentage.Px(12),
                                LengthPercentage.Px(8), LengthPercentage.Px(8)),
                            AlignItems = AlignItems.Center,
                        }
                    },
                    new BorderRadius { TopLeft = 6, TopRight = 6, BottomLeft = 6, BottomRight = 6 }
                )).Entity;

                // Toggle label
                var toggleText = commands.Spawn(Entity.With(
                    new UIText { Text = new NativeUtf8("Toggle"), Size = 14f, Color = Color.WHITE },
                    new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                    new UITextFont { Font = fontHandle },
                    new UIStyle { Value = LayoutStyle.Default }
                )).Entity;
                commands.Entity(toggle).AddChild(toggleText);
                commands.Entity(sidebar).AddChild(toggle);

                // Main panel with border - column layout for sections (scrollable)
                var mainPanel = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIScrollOffset(),
                    new UIInteraction(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexGrow = 1f,
                            FlexDirection = FlexDirection.Column,
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(16), LengthPercentage.Px(16),
                                LengthPercentage.Px(16), LengthPercentage.Px(16)),
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(16), LengthPercentage.Px(16)),
                            Border = new Rect<LengthPercentage>(
                                LengthPercentage.Px(2), LengthPercentage.Px(2),
                                LengthPercentage.Px(2), LengthPercentage.Px(2)),
                            Overflow = new Point<Overflow>(Overflow.Hidden, Overflow.Scroll),
                        }
                    },
                    new BackgroundColor { Color = new Color(0.15f, 0.15f, 0.19f, 1f) },
                    new BorderColor
                    {
                        Top = new Color(0.3f, 0.3f, 0.4f, 1f),
                        Right = new Color(0.3f, 0.3f, 0.4f, 1f),
                        Bottom = new Color(0.3f, 0.3f, 0.4f, 1f),
                        Left = new Color(0.3f, 0.3f, 0.4f, 1f),
                    },
                    new BorderRadius { TopLeft = 12, TopRight = 12, BottomLeft = 12, BottomRight = 12 }
                )).Entity;

                // --- Cards row ---
                var cardsRow = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Row,
                            FlexWrap = FlexWrap.Wrap,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(12), LengthPercentage.Px(12)),
                        }
                    }
                )).Entity;

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
                    var card = commands.Spawn(Entity.With(
                        new UINode(),
                        new UIInteraction { Focusable = true },
                        new UIButton
                        {
                            NormalColor = baseColor,
                            HoverColor = new Color(
                                baseColor.R * 0.7f + cardColors[i].R * 0.3f,
                                baseColor.G * 0.7f + cardColors[i].G * 0.3f,
                                baseColor.B * 0.7f + cardColors[i].B * 0.3f,
                                1f),
                            PressedColor = new Color(
                                cardColors[i].R * 0.6f,
                                cardColors[i].G * 0.6f,
                                cardColors[i].B * 0.6f,
                                1f),
                            DisabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f),
                        },
                        new BackgroundColor { Color = baseColor },
                        new UIStyle
                        {
                            Value = LayoutStyle.Default with
                            {
                                Size = new Size<Dimension>(Dimension.Px(160), Dimension.Px(120)),
                                FlexDirection = FlexDirection.Column,
                                Padding = new Rect<LengthPercentage>(
                                    LengthPercentage.Px(12), LengthPercentage.Px(12),
                                    LengthPercentage.Px(12), LengthPercentage.Px(12)),
                                Border = new Rect<LengthPercentage>(
                                    LengthPercentage.Px(2), LengthPercentage.Px(2),
                                    LengthPercentage.Px(2), LengthPercentage.Px(2)),
                            }
                        },
                        new BorderColor
                        {
                            Top = cardColors[i],
                            Right = cardColors[i],
                            Bottom = cardColors[i],
                            Left = cardColors[i],
                        },
                        new BorderRadius
                        {
                            TopLeft = cardRadii[i],
                            TopRight = cardRadii[i],
                            BottomLeft = cardRadii[i],
                            BottomRight = cardRadii[i],
                        }
                    )).Entity;

                    var cardTitle = commands.Spawn(Entity.With(
                        new UIText { Text = new NativeUtf8(cardLabels[i]), Size = 16f, Color = cardColors[i] },
                        new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                        new UITextFont { Font = fontHandle },
                        new UIStyle { Value = LayoutStyle.Default }
                    )).Entity;
                    commands.Entity(card).AddChild(cardTitle);

                    var cardBody = commands.Spawn(Entity.With(
                        new UIText { Text = new NativeUtf8("This is a card with text that may overflow the container bounds."), Size = 12f, Color = new Color(0.7f, 0.7f, 0.7f, 1f) },
                        new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                        new UITextFont { Font = fontHandle },
                        new UIStyle
                        {
                            Value = LayoutStyle.Default with
                            {
                                Margin = new Rect<LengthPercentageAuto>(
                                    LengthPercentageAuto.Zero, LengthPercentageAuto.Zero,
                                    LengthPercentageAuto.Px(8), LengthPercentageAuto.Zero),
                            }
                        }
                    )).Entity;
                    commands.Entity(card).AddChild(cardBody);

                    commands.Entity(cardsRow).AddChild(card);
                }

                commands.Entity(mainPanel).AddChild(cardsRow);

                // --- Helper: spawn a section label ---
                Entity SpawnLabel(string text, float size = 14f, Color? color = null)
                {
                    return commands.Spawn(Entity.With(
                        new UIText { Text = new NativeUtf8(text), Size = size, Color = color ?? Color.WHITE },
                        new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                        new UITextFont { Font = fontHandle },
                        new UIStyle { Value = LayoutStyle.Default }
                    )).Entity;
                }

                // --- CheckBox section ---
                var checkSection = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Column,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                        }
                    }
                )).Entity;

                commands.Entity(checkSection).AddChild(SpawnLabel("Checkboxes", 16f, new Color(0.5f, 0.8f, 1f, 1f)));

                string[] checkLabels = ["Enable notifications", "Dark mode", "Auto-save"];
                for (int i = 0; i < checkLabels.Length; i++)
                {
                    var row = commands.Spawn(Entity.With(
                        new UINode(),
                        new UIStyle
                        {
                            Value = LayoutStyle.Default with
                            {
                                FlexDirection = FlexDirection.Row,
                                AlignItems = AlignItems.Center,
                                Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                            }
                        }
                    )).Entity;

                    var cb = commands.Spawn(Entity.With(
                        new UINode(),
                        new UIInteraction { Focusable = true },
                        new UICheckBox { IsChecked = i == 2 },
                        new BackgroundColor { Color = i == 2 ? new Color(0.2f, 0.6f, 1.0f, 1f) : new Color(0.8f, 0.8f, 0.8f, 1f) },
                        new UIStyle
                        {
                            Value = LayoutStyle.Default with
                            {
                                Size = new Size<Dimension>(Dimension.Px(22), Dimension.Px(22)),
                            }
                        },
                        new BorderRadius { TopLeft = 4, TopRight = 4, BottomLeft = 4, BottomRight = 4 }
                    )).Entity;

                    commands.Entity(row).AddChild(cb);
                    commands.Entity(row).AddChild(SpawnLabel(checkLabels[i], 13f));
                    commands.Entity(checkSection).AddChild(row);
                }

                commands.Entity(mainPanel).AddChild(checkSection);

                // --- RadioButton section ---
                var radioSection = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Column,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                        }
                    }
                )).Entity;

                commands.Entity(radioSection).AddChild(SpawnLabel("Priority", 16f, new Color(0.5f, 0.8f, 1f, 1f)));

                string[] radioLabels = ["Low", "Medium", "High"];
                for (int i = 0; i < radioLabels.Length; i++)
                {
                    var row = commands.Spawn(Entity.With(
                        new UINode(),
                        new UIStyle
                        {
                            Value = LayoutStyle.Default with
                            {
                                FlexDirection = FlexDirection.Row,
                                AlignItems = AlignItems.Center,
                                Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                            }
                        }
                    )).Entity;

                    var rb = commands.Spawn(Entity.With(
                        new UINode(),
                        new UIInteraction { Focusable = true },
                        new UIRadioButton { GroupId = 1, IsSelected = i == 1 },
                        new BackgroundColor { Color = i == 1 ? new Color(0.2f, 0.6f, 1.0f, 1f) : new Color(0.8f, 0.8f, 0.8f, 1f) },
                        new UIShape { Type = UIShapeType.Circle },
                        new UIStyle
                        {
                            Value = LayoutStyle.Default with
                            {
                                Size = new Size<Dimension>(Dimension.Px(22), Dimension.Px(22)),
                            }
                        }
                    )).Entity;

                    commands.Entity(row).AddChild(rb);
                    commands.Entity(row).AddChild(SpawnLabel(radioLabels[i], 13f));
                    commands.Entity(radioSection).AddChild(row);
                }

                commands.Entity(mainPanel).AddChild(radioSection);

                // --- Slider section ---
                var sliderSection = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Column,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                        }
                    }
                )).Entity;

                commands.Entity(sliderSection).AddChild(SpawnLabel("Volume", 16f, new Color(0.5f, 0.8f, 1f, 1f)));

                var sliderEntity = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIInteraction { Focusable = true },
                    new UISlider { Min = 0, Max = 100, Step = 5, Value = 60 },
                    new BackgroundColor { Color = new Color(0.3f, 0.3f, 0.3f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(300), Dimension.Px(16)),
                        }
                    },
                    new BorderRadius { TopLeft = 8, TopRight = 8, BottomLeft = 8, BottomRight = 8 }
                )).Entity;

                commands.Entity(sliderSection).AddChild(sliderEntity);
                commands.Entity(mainPanel).AddChild(sliderSection);

                // --- TextInput section ---
                var textInputSection = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Column,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                        }
                    }
                )).Entity;

                commands.Entity(textInputSection).AddChild(SpawnLabel("Text Input", 16f, new Color(0.5f, 0.8f, 1f, 1f)));

                // Child text entity to display typed text
                var textInputText = commands.Spawn(Entity.With(
                    new UIText { Text = new NativeUtf8(""), Size = 14f, Color = Color.WHITE },
                    new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                    new UITextFont { Font = fontHandle },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Percent(1f), Dimension.Percent(1f)),
                        }
                    }
                )).Entity;

                var textInput = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIInteraction { Focusable = true },
                    new UITextInput { Filter = UIInputFilterType.Any, TextEntity = textInputText },
                    new BackgroundColor { Color = new Color(0.22f, 0.22f, 0.27f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(300), Dimension.Px(32)),
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(8), LengthPercentage.Px(8),
                                LengthPercentage.Px(6), LengthPercentage.Px(6)),
                            AlignItems = AlignItems.Center,
                            Border = new Rect<LengthPercentage>(
                                LengthPercentage.Px(1), LengthPercentage.Px(1),
                                LengthPercentage.Px(1), LengthPercentage.Px(1)),
                        }
                    },
                    new BorderColor
                    {
                        Top = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Right = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Bottom = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Left = new Color(0.4f, 0.4f, 0.5f, 1f),
                    },
                    new BorderRadius { TopLeft = 4, TopRight = 4, BottomLeft = 4, BottomRight = 4 }
                )).Entity;

                commands.Entity(textInput).AddChild(textInputText);
                commands.Entity(textInputSection).AddChild(textInput);
                commands.Entity(mainPanel).AddChild(textInputSection);

                // --- NumberInput section ---
                var numInputSection = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Column,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                        }
                    }
                )).Entity;

                commands.Entity(numInputSection).AddChild(SpawnLabel("Quantity (0-99)", 16f, new Color(0.5f, 0.8f, 1f, 1f)));

                // Child text entity for number input display
                var numInputText = commands.Spawn(Entity.With(
                    new UIText { Text = new NativeUtf8(""), Size = 14f, Color = Color.WHITE },
                    new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                    new UITextFont { Font = fontHandle },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Percent(1f), Dimension.Percent(1f)),
                        }
                    }
                )).Entity;

                var numTextInput = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIInteraction { Focusable = true },
                    new UITextInput { Filter = UIInputFilterType.Integer, TextEntity = numInputText },
                    new BackgroundColor { Color = new Color(0.22f, 0.22f, 0.27f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(120), Dimension.Px(32)),
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(8), LengthPercentage.Px(8),
                                LengthPercentage.Px(6), LengthPercentage.Px(6)),
                            AlignItems = AlignItems.Center,
                            Border = new Rect<LengthPercentage>(
                                LengthPercentage.Px(1), LengthPercentage.Px(1),
                                LengthPercentage.Px(1), LengthPercentage.Px(1)),
                        }
                    },
                    new BorderColor
                    {
                        Top = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Right = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Bottom = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Left = new Color(0.4f, 0.4f, 0.5f, 1f),
                    },
                    new BorderRadius { TopLeft = 4, TopRight = 4, BottomLeft = 4, BottomRight = 4 }
                )).Entity;

                commands.Entity(numTextInput).AddChild(numInputText);

                var numInput = commands.Spawn(Entity.With(
                    new UINode(),
                    new UINumberInput { Min = 0, Max = 99, Step = 1, Type = NumberInputType.Int, TextInputEntity = numTextInput },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(120), Dimension.Px(32)),
                        }
                    }
                )).Entity;

                commands.Entity(numInputSection).AddChild(numInput);
                commands.Entity(numInputSection).AddChild(numTextInput);
                commands.Entity(mainPanel).AddChild(numInputSection);

                // --- Dropdown section ---
                var dropdownSection = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Column,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                        }
                    }
                )).Entity;

                commands.Entity(dropdownSection).AddChild(SpawnLabel("Theme", 16f, new Color(0.5f, 0.8f, 1f, 1f)));

                // Display text for the dropdown trigger (shows selected option)
                string[] optionLabels = ["Dark", "Light", "Solarized", "Nord"];
                var dropdownDisplayText = commands.Spawn(Entity.With(
                    new UIText { Text = new NativeUtf8(optionLabels[0]), Size = 12f, Color = Color.WHITE },
                    new TextMesh { Mesh = Handle<TextMeshAsset>.Null },
                    new UITextFont { Font = fontHandle },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Percent(1f), Dimension.Percent(1f)),
                        }
                    }
                )).Entity;

                var dropdown = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIInteraction { Focusable = true },
                    new UIDropdown { SelectedIndex = 0, DisplayTextEntity = dropdownDisplayText },
                    new UIButton
                    {
                        NormalColor = new Color(0.25f, 0.25f, 0.30f, 1f),
                        HoverColor = new Color(0.30f, 0.30f, 0.40f, 1f),
                        PressedColor = new Color(0.20f, 0.20f, 0.28f, 1f),
                        DisabledColor = new Color(0.20f, 0.20f, 0.22f, 0.5f),
                    },
                    new BackgroundColor { Color = new Color(0.25f, 0.25f, 0.30f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(32)),
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(10), LengthPercentage.Px(10),
                                LengthPercentage.Px(6), LengthPercentage.Px(6)),
                            AlignItems = AlignItems.Center,
                            Border = new Rect<LengthPercentage>(
                                LengthPercentage.Px(1), LengthPercentage.Px(1),
                                LengthPercentage.Px(1), LengthPercentage.Px(1)),
                        }
                    },
                    new BorderColor
                    {
                        Top = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Right = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Bottom = new Color(0.4f, 0.4f, 0.5f, 1f),
                        Left = new Color(0.4f, 0.4f, 0.5f, 1f),
                    },
                    new BorderRadius { TopLeft = 4, TopRight = 4, BottomLeft = 4, BottomRight = 4 }
                )).Entity;

                commands.Entity(dropdown).AddChild(dropdownDisplayText);

                // Dropdown trigger comes first in the column, options float below when open
                commands.Entity(dropdownSection).AddChild(dropdown);

                // Options panel: absolutely positioned below the trigger
                var optionsPanel = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIDropdownOptionTag { DropdownEntity = dropdown, OptionIndex = -1 },
                    new BackgroundColor { Color = new Color(0.18f, 0.18f, 0.22f, 1f) },
                    new BorderColor
                    {
                        Top = new Color(0.35f, 0.35f, 0.45f, 1f),
                        Right = new Color(0.35f, 0.35f, 0.45f, 1f),
                        Bottom = new Color(0.35f, 0.35f, 0.45f, 1f),
                        Left = new Color(0.35f, 0.35f, 0.45f, 1f),
                    },
                    new BorderRadius { TopLeft = 4, TopRight = 4, BottomLeft = 4, BottomRight = 4 },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Display = Display.None,
                            Position = Position.Absolute,
                            Overflow = new Point<Overflow>(Overflow.Hidden, Overflow.Hidden),
                            Inset = new Rect<LengthPercentageAuto>(
                                LengthPercentageAuto.Px(0),
                                LengthPercentageAuto.Auto,
                                LengthPercentageAuto.Px(40), // below trigger (32px) + gap (8px)
                                LengthPercentageAuto.Auto),
                            FlexDirection = FlexDirection.Column,
                            MaxSize = new Size<Dimension>(Dimension.Auto, Dimension.Px(200)),
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(2), LengthPercentage.Px(2),
                                LengthPercentage.Px(2), LengthPercentage.Px(2)),
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(2), LengthPercentage.Px(2)),
                            Border = new Rect<LengthPercentage>(
                                LengthPercentage.Px(1), LengthPercentage.Px(1),
                                LengthPercentage.Px(1), LengthPercentage.Px(1)),
                        }
                    }
                )).Entity;
                commands.Entity(dropdownSection).AddChild(optionsPanel);

                // Dropdown options (hidden by default via panel)
                for (int i = 0; i < optionLabels.Length; i++)
                {
                    var opt = commands.Spawn(Entity.With(
                        new UINode(),
                        new UIInteraction { Focusable = true },
                        new UIDropdownOptionTag { DropdownEntity = dropdown, OptionIndex = i },
                        new UIButton
                        {
                            NormalColor = new Color(0.20f, 0.20f, 0.25f, 1f),
                            HoverColor = new Color(0.30f, 0.30f, 0.38f, 1f),
                            PressedColor = new Color(0.15f, 0.15f, 0.20f, 1f),
                            DisabledColor = new Color(0.20f, 0.20f, 0.22f, 0.5f),
                        },
                        new BackgroundColor { Color = new Color(0.20f, 0.20f, 0.25f, 1f) },
                        new UIStyle
                        {
                            Value = LayoutStyle.Default with
                            {
                                Display = Display.None,
                                Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(28)),
                                Padding = new Rect<LengthPercentage>(
                                    LengthPercentage.Px(10), LengthPercentage.Px(10),
                                    LengthPercentage.Px(4), LengthPercentage.Px(4)),
                                AlignItems = AlignItems.Center,
                            }
                        }
                    )).Entity;

                    commands.Entity(opt).AddChild(SpawnLabel(optionLabels[i], 12f));
                    commands.Entity(optionsPanel).AddChild(opt);
                }

                commands.Entity(mainPanel).AddChild(dropdownSection);

                // --- SDF Shapes demo ---
                var shapesSection = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Column,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(8), LengthPercentage.Px(8)),
                        }
                    }
                )).Entity;

                commands.Entity(shapesSection).AddChild(SpawnLabel("SDF Shapes", 16f, new Color(0.5f, 0.8f, 1f, 1f)));

                var shapesRow = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Row,
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(16), LengthPercentage.Px(16)),
                            AlignItems = AlignItems.Center,
                        }
                    }
                )).Entity;

                // Rounded rect (default)
                var rectShape = commands.Spawn(Entity.With(
                    new UINode(),
                    new BackgroundColor { Color = new Color(0.9f, 0.3f, 0.3f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(48), Dimension.Px(48)),
                        }
                    },
                    new BorderRadius { TopLeft = 8, TopRight = 8, BottomLeft = 8, BottomRight = 8 }
                )).Entity;
                commands.Entity(shapesRow).AddChild(rectShape);

                // Circle
                var circleShape = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIShape { Type = UIShapeType.Circle },
                    new BackgroundColor { Color = new Color(0.3f, 0.8f, 0.4f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(48), Dimension.Px(48)),
                        }
                    }
                )).Entity;
                commands.Entity(shapesRow).AddChild(circleShape);

                // Checkmark
                var checkShape = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIShape { Type = UIShapeType.Checkmark },
                    new BackgroundColor { Color = new Color(0.3f, 0.5f, 0.9f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(48), Dimension.Px(48)),
                        }
                    }
                )).Entity;
                commands.Entity(shapesRow).AddChild(checkShape);

                // Down arrow
                var arrowShape = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIShape { Type = UIShapeType.DownArrow },
                    new BackgroundColor { Color = new Color(0.9f, 0.7f, 0.2f, 1f) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Dimension>(Dimension.Px(48), Dimension.Px(48)),
                        }
                    }
                )).Entity;
                commands.Entity(shapesRow).AddChild(arrowShape);

                commands.Entity(shapesSection).AddChild(shapesRow);
                commands.Entity(mainPanel).AddChild(shapesSection);

                // Build hierarchy
                root.AddChild(header);
                root.AddChild(contentRow);
                commands.Entity(contentRow).AddChild(sidebar);
                commands.Entity(contentRow).AddChild(mainPanel);
            }))
        .Build()).Run();
}
