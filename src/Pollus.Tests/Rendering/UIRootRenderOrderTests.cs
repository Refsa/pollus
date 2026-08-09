namespace Pollus.Tests.Rendering;

using Pollus.Assets;
using Pollus.ECS;
using Pollus.Engine.Rendering;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

public class UIRootRenderOrderTests
{
    [Fact]
    public void ExtractUIRects_OrdersRootsByZIndex()
    {
        using var world = new World();

        Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(100, 100), ZIndex = 10f },
            new ComputedNode { Size = new Vec2f(100, 100) },
            new BackgroundColor { Color = Color.RED }
        ).Spawn(world);

        Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(100, 100), ZIndex = -10f },
            new ComputedNode { Size = new Vec2f(100, 100) },
            new BackgroundColor { Color = Color.BLUE }
        ).Spawn(world);

        using var batches = new UIRectBatches();
        using var meshes = new Assets<TextMeshAsset>();
        var roots = new Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>>(world);

        ExtractUIRectsSystem.ExtractUIRects(
            Local.From(new ExtractUIRectsSystem.LocalData()),
            batches,
            new UIRenderResources(),
            meshes,
            new Query(world),
            roots);

        var batch = Assert.IsType<UIRectBatch>(Assert.Single(batches.Batches.ToArray()));
        Assert.Equal((Vec4f)Color.BLUE, batch.Data[0].BackgroundColor);
        Assert.Equal((Vec4f)Color.RED, batch.Data[1].BackgroundColor);
    }
}
