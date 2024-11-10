namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Mathematics;

public interface ITransform
{
    Mat4f ToMat4f();
    GlobalTransform ToGlobalTransform(Mat4f parentTransform);
}

public class TransformPlugin<TTransform> : IPlugin
    where TTransform : unmanaged, ITransform, IComponent
{
    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.PostUpdate, FnSystem.Create(
            "Transform::Propagate",
            static (
                Query query,
                Query<GlobalTransform, TTransform, Parent>.Filter<None<Child>> qRoots,
                Query<GlobalTransform, TTransform, Child> qChildren,
                Query<GlobalTransform, TTransform> qAllTransforms
            ) =>
            {
                qAllTransforms.ForEach(static (ref GlobalTransform globalTransform, ref TTransform transform) =>
                {
                    globalTransform.Value = transform.ToMat4f();
                });

                qRoots.ForEach(query, static (in Query query, in Entity root, ref GlobalTransform globalTransform, ref TTransform transform, ref Parent parent) =>
                {
                    Propagate(root, query, Mat4f.Identity());
                });

                static void Propagate(in Entity current, in Query query, in Mat4f parentTransform)
                {
                    if (query.Has<GlobalTransform>(current))
                    {
                        ref var globalTransform = ref query.Get<GlobalTransform>(current);
                        ref var transform = ref query.Get<TTransform>(current);
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
        ));
    }
}
