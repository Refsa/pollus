namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Rendering;
using Pollus.Engine.UI;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
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
            new UIRenderPlugin(),
            new UIWidgetPlugin(),
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
                        }
                    },
                    new BackgroundColor { Color = new Color(0.2f, 0.4f, 0.8f, 1f) },
                    new BorderRadius { TopLeft = 8, TopRight = 8, BottomLeft = 8, BottomRight = 8 }
                )).Entity;

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
                            }
                        },
                        new BorderRadius { TopLeft = 6, TopRight = 6, BottomLeft = 6, BottomRight = 6 }
                    )).Entity;

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
                        }
                    },
                    new BorderRadius { TopLeft = 6, TopRight = 6, BottomLeft = 6, BottomRight = 6 }
                )).Entity;
                commands.Entity(sidebar).AddChild(toggle);

                // Main panel with border
                var mainPanel = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexGrow = 1f,
                            FlexDirection = FlexDirection.Row,
                            FlexWrap = FlexWrap.Wrap,
                            Padding = new Rect<LengthPercentage>(
                                LengthPercentage.Px(16), LengthPercentage.Px(16),
                                LengthPercentage.Px(16), LengthPercentage.Px(16)),
                            Gap = new Size<LengthPercentage>(LengthPercentage.Px(12), LengthPercentage.Px(12)),
                            Border = new Rect<LengthPercentage>(
                                LengthPercentage.Px(2), LengthPercentage.Px(2),
                                LengthPercentage.Px(2), LengthPercentage.Px(2)),
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

                // Interactive cards - hover/press to see color feedback
                Color[] cardColors =
                [
                    new(0.9f, 0.3f, 0.3f, 1f),
                    new(0.3f, 0.8f, 0.4f, 1f),
                    new(0.3f, 0.5f, 0.9f, 1f),
                ];
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

                    commands.Entity(mainPanel).AddChild(card);
                }

                // Build hierarchy
                root.AddChild(header);
                root.AddChild(contentRow);
                commands.Entity(contentRow).AddChild(sidebar);
                commands.Entity(contentRow).AddChild(mainPanel);
            }))
        .Build()).Run();
}
