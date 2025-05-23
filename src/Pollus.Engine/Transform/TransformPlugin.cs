namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Mathematics;

public interface ITransform
{
    Mat4f ToMat4f();
    GlobalTransform ToGlobalTransform(Mat4f parentTransform);
}

[SystemSet]
public partial class TransformPlugin<TTransform> : IPlugin
    where TTransform : unmanaged, ITransform, IComponent
{
    [System(nameof(Propagate))]
    static readonly SystemBuilderDescriptor PropagateSystemDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
    };

    static void Propagate(
        Query query,
        Query<GlobalTransform, Read<TTransform>, Read<Parent>>.Filter<None<Child>> qRoots,
        Query<GlobalTransform, Read<TTransform>>.Filter<Any<Parent, Child>> qTreeTransforms,
        Query<GlobalTransform, Read<TTransform>>.Filter<None<Parent, Child>> qOrphans
    )
    {
        qOrphans.ForEach(static (ref GlobalTransform globalTransform, ref Read<TTransform> transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
        });

        qTreeTransforms.ForEach(static (ref GlobalTransform globalTransform, ref Read<TTransform> transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
        });

        qRoots.ForEach(query, static (in Query query, in Entity root, ref GlobalTransform globalTransform, ref Read<TTransform> transform, ref Read<Parent> parent) =>
        {
            Propagate(root, query, Mat4f.Identity());
        });

        static void Propagate(in Entity current, in Query query, in Mat4f parentTransform)
        {
            {
                ref var child = ref query.TryGet<Child>(current, out var hasChild);
                if (hasChild)
                {
                    if (!child.NextSibling.IsNull)
                    {
                        Propagate(child.NextSibling, query, parentTransform);
                    }
                }
            }

            {
                ref var globalTransform = ref query.TryGet<GlobalTransform>(current, out var hasGlobalTransform);
                if (hasGlobalTransform)
                {
                    globalTransform.Value = parentTransform * globalTransform.Value;

                    ref var parent = ref query.TryGet<Parent>(current, out var hasParent);
                    if (hasParent)
                    {
                        Propagate(parent.FirstChild, query, globalTransform.Value);
                    }
                }
            }
        }
    }

    public void Apply(World world)
    {
        world.Schedule.AddSystemSet<TransformPlugin<TTransform>>();
    }
}
