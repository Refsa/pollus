namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Mathematics;

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
                Query<GlobalTransform, TTransform>.Filter<None<Parent, Child>> qOrphans
            ) =>
            {
                foreach (var root in qRoots)
                {
                    PropagateDown(root.Entity, query, Mat4f.Identity());
                }

                foreach (var orphan in qOrphans)
                {
                    orphan.Component0.Value = orphan.Component1.ToMat4f();
                }

                static void PropagateDown(in Entity current, in Query query, in Mat4f parentTransform)
                {
                    ref var globalTransform = ref query.Get<GlobalTransform>(current);
                    ref var transform = ref query.Get<TTransform>(current);
                    globalTransform.Value = parentTransform * transform.ToMat4f();

                    if (query.Has<Parent>(current))
                    {
                        PropagateDown(query.Get<Parent>(current).FirstChild, query, globalTransform.Value);
                    }

                    if (query.Has<Child>(current))
                    {
                        ref var child = ref query.Get<Child>(current);
                        if (child.NextSibling.IsNull) return;
                        PropagateDown(child.NextSibling, query, parentTransform);
                    }
                }
            }
        ));
    }
}
