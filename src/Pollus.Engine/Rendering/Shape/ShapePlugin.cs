namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public class ShapePlugin : IPlugin
{
    static ShapePlugin()
    {
        AssetsFetch<Shape>.Register();
    }

    public PluginDependency[] Dependencies => [
        PluginDependency.From<RenderingPlugin>(),
    ];

    public void Apply(World world)
    {
        world.AddPlugin(new MaterialPlugin<ShapeMaterial>());
        world.Resources.Add(new ShapeBatches());
        world.Resources.Get<RenderAssets>().AddLoader(new ShapeRenderDataLoader());

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractShapeDrawSystem(),
            new WriteBatchesSystem<ShapeBatches, ShapeBatch>(),
            new DrawBatchesSystem<ShapeBatches, ShapeBatch>()
            {
                RenderStep = RenderStep2D.Main,
                DrawExec = static (renderAssets, batch) =>
                {
                    var material = renderAssets.Get<MaterialRenderData>(batch.Material);
                    var shape = renderAssets.Get<ShapeRenderData>(batch.Shape);

                    var draw = Draw.Create(material.Pipeline)
                        .SetVertexInfo(shape.VertexCount, 0)
                        .SetInstanceInfo((uint)batch.Count, 0)
                        .SetVertexBuffer(0, shape.VertexBuffer)
                        .SetVertexBuffer(1, batch.InstanceBufferHandle)
                        .SetBindGroups(material.BindGroups);

                    return draw;
                },
            },
        ]);
    }
}

public partial struct ShapeDraw : IComponent
{
    public static EntityBuilder<ShapeDraw, Transform2D, GlobalTransform> Bundle => new(
        new()
        {
            MaterialHandle = Handle<ShapeMaterial>.Null,
            ShapeHandle = Handle<Shape>.Null,
            Color = Color.WHITE,
            Offset = Vec2f.Zero,
        },
        Transform2D.Default,
        GlobalTransform.Default
    );

    public required Handle<ShapeMaterial> MaterialHandle;
    public required Handle<Shape> ShapeHandle;
    public required Color Color;
    public Vec2f Offset;
}
