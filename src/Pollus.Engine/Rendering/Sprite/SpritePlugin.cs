namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
using Transform;

public class SpritePlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
        PluginDependency.From<TransformPlugin<Transform2D>>(),
    ];

    public void Apply(World world)
    {
        world.AddPlugins(true, [
            new MaterialPlugin<SpriteMaterial>(),
        ]);
        world.Resources.Add(new SpriteBatches());

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractSpritesSystem(),
            new SortBatchesSystem<SpriteBatches, SpriteBatch, SpriteBatch.InstanceData>()
            {
                Compare = static (a, b) => a.Model2.W.CompareTo(b.Model2.W),
            },
            new WriteBatchesSystem<SpriteBatches, SpriteBatch>(),
            new DrawBatchesSystem<SpriteBatches, SpriteBatch>()
            {
                RenderStep = RenderStep2D.Main,
                DrawExec = static (renderAssets, batch) =>
                {
                    var material = renderAssets.Get<MaterialRenderData>(batch.Material);
                    return Draw.Create(material.Pipeline)
                        .SetVertexInfo(6, 0)
                        .SetInstanceInfo((uint)batch.Count, 0)
                        .SetVertexBuffer(0, batch.InstanceBufferHandle)
                        .SetBindGroups(material.BindGroups);
                },
            },
        ]);
    }
}