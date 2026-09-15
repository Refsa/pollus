namespace Pollus.Tests.Rendering;

using Pollus.Assets;
using Pollus.ECS;
using Pollus.Engine.Rendering;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

public class UIScissorTests
{
    [Fact]
    public void ScrolledContentKeepsTheViewportClipWhenThePanelMoves()
    {
        using var world = new World();
        var commands = world.GetCommands();
        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800f, 600f) },
            new ComputedNode { Size = new Vec2f(800f, 600f) })).Entity;
        var panel = SpawnNode(commands, new Vec2f(40f, 60f), new Vec2f(300f, 280f));
        var viewport = SpawnNode(commands, new Vec2f(16f, 28f), new Vec2f(200f, 120f), Overflow.Scroll);
        commands.AddComponent(viewport, new UIScrollOffset { Offset = new Vec2f(20f, 40f) });
        var content = SpawnNode(commands, Vec2f.Zero, new Vec2f(300f, 300f));
        commands.AddComponent(content, new BackgroundColor { Color = Color.RED });
        var footer = SpawnNode(commands, new Vec2f(0f, 260f), new Vec2f(300f, 20f));
        commands.AddComponent(footer, new BackgroundColor { Color = Color.BLUE });
        commands.AddChild(root, panel);
        commands.AddChild(panel, viewport);
        commands.AddChild(viewport, content);
        commands.AddChild(panel, footer);
        commands.Flush(world);
        ref var node = ref world.Store.GetComponent<ComputedNode>(viewport);
        node.BorderLeft = 2f;
        node.BorderTop = 3f;
        node.BorderRight = 4f;
        node.BorderBottom = 5f;

        foreach (var position in new[] { new Vec2f(40f, 60f), new Vec2f(140f, 100f) })
        {
            world.Store.GetComponent<ComputedNode>(panel).Position = position;
            using var batches = Extract(world);
            var clipped = Assert.Single(batches.Batches.ToArray().Cast<UIRectBatch>(), batch => batch.ScissorRect.HasValue);
            var unclipped = Assert.Single(batches.Batches.ToArray().Cast<UIRectBatch>(), batch => !batch.ScissorRect.HasValue);
            Assert.Equal(new Rect<int>((int)position.X + 18, (int)position.Y + 31, 194, 112), clipped.ScissorRect);
            Assert.Equal(new Vec4f(position.X - 4f, position.Y - 12f, 300f, 300f), clipped.Data[0].PosSize);
            Assert.Equal((Vec4f)Color.RED, clipped.Data[0].BackgroundColor);
            Assert.Equal((Vec4f)Color.BLUE, unclipped.Data[0].BackgroundColor);
        }
    }

    [Fact]
    public void NestedScrollAreasIntersectClipsAndCullDisjointContent()
    {
        using var world = new World();
        var commands = world.GetCommands();
        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800f, 600f) },
            new ComputedNode { Size = new Vec2f(800f, 600f) })).Entity;
        var outer = SpawnNode(commands, new Vec2f(100f, 80f), new Vec2f(200f, 160f), Overflow.Scroll);
        commands.AddComponent(outer, new UIScrollOffset { Offset = new Vec2f(20f, 10f) });
        var inner = SpawnNode(commands, new Vec2f(140f, 120f), new Vec2f(180f, 140f), Overflow.Scroll);
        commands.AddComponent(inner, new UIScrollOffset { Offset = new Vec2f(0f, 30f) });
        var content = SpawnNode(commands, Vec2f.Zero, new Vec2f(180f, 300f));
        commands.AddComponent(content, new BackgroundColor { Color = Color.RED });
        commands.AddChild(root, outer);
        commands.AddChild(outer, inner);
        commands.AddChild(inner, content);
        commands.Flush(world);

        using (var batches = Extract(world))
        {
            var batch = Assert.IsType<UIRectBatch>(Assert.Single(batches.Batches.ToArray()));
            Assert.Equal(new Rect<int>(220, 190, 80, 50), batch.ScissorRect);
            Assert.Equal(new Vec4f(220f, 160f, 180f, 300f), batch.Data[0].PosSize);
        }

        world.Store.GetComponent<ComputedNode>(inner).Position = new Vec2f(250f, 200f);
        using var hiddenBatches = Extract(world);
        Assert.Empty(hiddenBatches.Batches.ToArray());
    }

    static Entity SpawnNode(Commands commands, Vec2f position, Vec2f size, Overflow overflow = Overflow.Visible)
    {
        return commands.Spawn(Entity.With(
            new UINode(),
            new ComputedNode { Position = position, Size = size },
            new UIStyle { Value = Style.Default with { Overflow = new Point<Overflow>(overflow, overflow) } })).Entity;
    }

    static UIRectBatches Extract(World world)
    {
        var batches = new UIRectBatches();
        using var meshes = new Assets<TextMeshAsset>();
        ExtractUIRectsSystem.ExtractUIRects(Local.From(new ExtractUIRectsSystem.LocalData()), batches, new UIRenderResources(), meshes,
            new Query(world), new Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>>(world));
        return batches;
    }
}
