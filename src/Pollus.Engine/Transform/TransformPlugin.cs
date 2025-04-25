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
        Query<GlobalTransform, Read<TTransform>> qAllTransforms
    )
    {
        qAllTransforms.ForEach(static (ref GlobalTransform globalTransform, ref Read<TTransform> transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
        });

        qRoots.ForEach(query, static (in Query query, in Entity root, ref GlobalTransform globalTransform, ref Read<TTransform> transform, ref Read<Parent> parent) =>
        {
            Propagate(root, query, Mat4f.Identity());
        });

        static void Propagate(in Entity current, in Query query, in Mat4f parentTransform)
        {
            if (query.Has<GlobalTransform>(current))
            {
                ref var globalTransform = ref query.Get<GlobalTransform>(current);
                globalTransform.Value = parentTransform * globalTransform.Value;

                if (query.Has<Parent>(current))
                {
                    Propagate(query.Get<Parent>(current).FirstChild, query, globalTransform.Value);
                }
            }

            if (query.Has<Child>(current))
            {
                ref var child = ref query.Get<Child>(current);
                if (child.NextSibling.IsNull) return;
                Propagate(child.NextSibling, query, parentTransform);
            }
        }
    }

    public void Apply(World world)
    {
        world.Schedule.AddSystemSet<TransformPlugin<TTransform>>();
    }
}
