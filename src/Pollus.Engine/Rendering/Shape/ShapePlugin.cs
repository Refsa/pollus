namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Utils;

public class ShapePlugin : IPlugin
{
    static ShapePlugin()
    {
        AssetsFetch<Shape>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<ShapeMaterial>());
        world.Resources.Get<RenderAssets>().AddLoader(new ShapeRenderDataLoader());
        world.Resources.Add(new ShapeBatches());

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractShapesSystem(),
            new WriteShapeBatchesSystem(),
            new DrawShapeBatchesSystem(),
        ]);
    }
}

public struct ShapeDraw : IComponent
{
    public static EntityBuilder<ShapeDraw, Transform2> Bundle => new(
        new()
        {
            MaterialHandle = Handle<ShapeMaterial>.Null,
            ShapeHandle = Handle<Shape>.Null,
            Color = Color.WHITE,
        },
        Transform2.Default
    );

    public required Handle<ShapeMaterial> MaterialHandle;
    public required Handle<Shape> ShapeHandle;
    public required Color Color;
}
