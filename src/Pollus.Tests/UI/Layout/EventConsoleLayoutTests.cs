using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

/// Reproduces the UIExample event console layout to diagnose sizing issues.
public class EventConsoleLayoutTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void EventConsole_FillsInspectorColumn()
    {
        // Replicates the hierarchy:
        // root (Column, 800x600, padding 16, gap 16)
        //   header (height 80)
        //   workspace (FlexGrow 1, Row, gap 16, minSize 0)
        //     showcaseSurface (FlexGrow 1, Column, minSize 0)
        //       contentRow (FlexGrow 1, Row, gap 16, minSize 0)
        //         sidebar (width 210, Column)
        //         mainPanel (FlexGrow 1, Column, Overflow(Scroll,Scroll), minSize 0, padding 16, border 2)
        //           [sections that overflow]
        //     inspectorColumn (width 360, FlexShrink 0, Column, Overflow(Hidden,Hidden), minSize 0, padding 12, border 1)
        //       eventConsoleWrapper (FlexGrow 1, Column, gap 8, minSize 0, padding 12, border 1)
        //         title (measured ~20px)
        //         description (measured ~18px)
        //         eventLogViewport (FlexGrow 1, Row(default), minSize 0, Overflow(Hidden,Scroll), padding 10, border 1)
        //           eventLogText (width 100%, height auto = measured ~200px)

        var tree = new TestLayoutTree();

        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            Size = new Size<Length>(Length.Px(800), Length.Px(600)),
            Padding = Rect<Length>.All(Length.Px(16)),
            Gap = new Size<Length>(Length.Px(16), Length.Px(16)),
        });

        var header = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Auto, Length.Px(80)),
        });

        var workspace = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
            FlexDirection = FlexDirection.Row,
            Gap = new Size<Length>(Length.Px(16), Length.Px(16)),
            MinSize = new Size<Length>(Length.Px(0), Length.Px(0)),
        });

        // Left side - just a simple panel with some content
        var showcaseSurface = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
            FlexDirection = FlexDirection.Column,
            MinSize = new Size<Length>(Length.Px(0), Length.Px(0)),
        });

        var contentRow = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
            FlexDirection = FlexDirection.Row,
            Gap = new Size<Length>(Length.Px(16), Length.Px(16)),
            MinSize = new Size<Length>(Length.Px(0), Length.Px(0)),
        });

        var sidebar = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Px(210), Length.Auto),
            FlexDirection = FlexDirection.Column,
        });
        // Some sidebar content
        for (int i = 0; i < 3; i++)
        {
            var btn = tree.AddNode(DefaultStyle with
            {
                Size = new Size<Length>(Length.Auto, Length.Px(40)),
            });
            tree.AddChild(sidebar, btn);
        }

        var mainPanel = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
            FlexDirection = FlexDirection.Column,
            Overflow = new Point<Overflow>(Overflow.Scroll, Overflow.Scroll),
            MinSize = new Size<Length>(Length.Px(0), Length.Px(0)),
            Padding = Rect<Length>.All(Length.Px(16)),
            Border = Rect<Length>.All(Length.Px(2)),
            Gap = new Size<Length>(Length.Px(16), Length.Px(16)),
        });
        // Lots of content that overflows
        for (int i = 0; i < 10; i++)
        {
            var section = tree.AddNode(DefaultStyle with
            {
                Size = new Size<Length>(Length.Auto, Length.Px(100)),
            });
            tree.AddChild(mainPanel, section);
        }

        // Right side - the inspector column
        var inspectorColumn = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Px(360), Length.Auto),
            FlexShrink = 0f,
            FlexDirection = FlexDirection.Column,
            Gap = new Size<Length>(Length.Px(0), Length.Px(0)),
            Overflow = new Point<Overflow>(Overflow.Hidden, Overflow.Hidden),
            MinSize = new Size<Length>(Length.Px(0), Length.Px(0)),
            Padding = Rect<Length>.All(Length.Px(12)),
            Border = Rect<Length>.All(Length.Px(1)),
        });

        var eventConsoleWrapper = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
            FlexDirection = FlexDirection.Column,
            Gap = new Size<Length>(Length.Px(8), Length.Px(8)),
            MinSize = new Size<Length>(Length.Px(0), Length.Px(0)),
            Padding = Rect<Length>.All(Length.Px(12)),
            Border = Rect<Length>.All(Length.Px(1)),
        });

        // Title text
        var title = tree.AddNode(DefaultStyle);
        tree.SetMeasureFunc(title, input =>
        {
            float w = input.KnownDimensions.Width ?? 120f;
            float h = input.KnownDimensions.Height ?? 20f;
            return new LayoutOutput { Size = new Size<float>(w, h) };
        });

        // Description text
        var desc = tree.AddNode(DefaultStyle);
        tree.SetMeasureFunc(desc, input =>
        {
            float w = input.KnownDimensions.Width ?? 300f;
            float h = input.KnownDimensions.Height ?? 18f;
            return new LayoutOutput { Size = new Size<float>(w, h) };
        });

        // Event log viewport
        var eventLogViewport = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
            // Default FlexDirection = Row
            MinSize = new Size<Length>(Length.Px(0), Length.Px(0)),
            Overflow = new Point<Overflow>(Overflow.Hidden, Overflow.Scroll),
            Padding = Rect<Length>.All(Length.Px(10)),
            Border = Rect<Length>.All(Length.Px(1)),
        });

        // Event log text (multi-line, measured) - tall enough to require scrolling
        var eventLogText = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Percent(1f), Length.Auto),
        });
        tree.SetMeasureFunc(eventLogText, input =>
        {
            float w = input.KnownDimensions.Width ?? 200f;
            float h = input.KnownDimensions.Height ?? 500f; // many lines of event log text
            return new LayoutOutput { Size = new Size<float>(w, h) };
        });

        // Build hierarchy
        tree.AddChild(root, header);
        tree.AddChild(root, workspace);
        tree.AddChild(workspace, showcaseSurface);
        tree.AddChild(workspace, inspectorColumn);
        tree.AddChild(showcaseSurface, contentRow);
        tree.AddChild(contentRow, sidebar);
        tree.AddChild(contentRow, mainPanel);
        tree.AddChild(inspectorColumn, eventConsoleWrapper);
        tree.AddChild(eventConsoleWrapper, title);
        tree.AddChild(eventConsoleWrapper, desc);
        tree.AddChild(eventConsoleWrapper, eventLogViewport);
        tree.AddChild(eventLogViewport, eventLogText);

        tree.ComputeRoot(root, 800, 600);

        var rootLayout = tree.GetNodeLayout(root);
        var workspaceLayout = tree.GetNodeLayout(workspace);
        var inspectorLayout = tree.GetNodeLayout(inspectorColumn);
        var wrapperLayout = tree.GetNodeLayout(eventConsoleWrapper);
        var viewportLayout = tree.GetNodeLayout(eventLogViewport);
        var textLayout = tree.GetNodeLayout(eventLogText);

        // Root: 800x600
        Assert.Equal(800f, rootLayout.Size.Width);
        Assert.Equal(600f, rootLayout.Size.Height);

        var headerLayout = tree.GetNodeLayout(header);

        // Header should keep its specified 80px height (not shrink due to scroll container inflation)
        Assert.Equal(80f, headerLayout.Size.Height);

        // Workspace: fills remaining space = 600 - 32(padding) - 16(gap) - 80(header) = 472
        Assert.Equal(472f, workspaceLayout.Size.Height);

        // Inspector column: stretched to workspace height
        Assert.Equal(360f, inspectorLayout.Size.Width);
        Assert.Equal(workspaceLayout.Size.Height, inspectorLayout.Size.Height);

        // Event console wrapper: should fill the inspector column (minus padding+border)
        float inspectorContentH = inspectorLayout.Size.Height
                                  - inspectorLayout.Padding.Top - inspectorLayout.Padding.Bottom
                                  - inspectorLayout.Border.Top - inspectorLayout.Border.Bottom;
        Assert.Equal(inspectorContentH, wrapperLayout.Size.Height);

        // Viewport: should fill the wrapper (minus title, desc, gaps, padding+border)
        float wrapperContentH = wrapperLayout.Size.Height
                                - wrapperLayout.Padding.Top - wrapperLayout.Padding.Bottom
                                - wrapperLayout.Border.Top - wrapperLayout.Border.Bottom;
        float titleH = tree.GetNodeLayout(title).Size.Height;
        float descH = tree.GetNodeLayout(desc).Size.Height;
        float expectedViewportH = wrapperContentH - titleH - descH - 8 * 2; // 2 gaps
        Assert.True(viewportLayout.Size.Height > 100f,
            $"Viewport height ({viewportLayout.Size.Height}) should be > 100 (fill remaining space), expected ~{expectedViewportH}");
        Assert.Equal(expectedViewportH, viewportLayout.Size.Height);

        // Text: measured height, not constrained to viewport
        Assert.Equal(500f, textLayout.Size.Height);

        // ContentSize of viewport should exceed its inner height (enabling scroll)
        float viewportInnerH = viewportLayout.Size.Height
                               - viewportLayout.Padding.Top - viewportLayout.Padding.Bottom
                               - viewportLayout.Border.Top - viewportLayout.Border.Bottom;
        Assert.True(viewportLayout.ContentSize.Height > viewportInnerH,
            $"Viewport ContentSize.Height ({viewportLayout.ContentSize.Height}) should exceed inner height ({viewportInnerH})");
    }
}
