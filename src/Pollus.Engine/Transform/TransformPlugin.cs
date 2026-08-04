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
    [System(CoreStage.PostUpdate)]
    static void HandleDynamic(
        Query query,
        Query<GlobalTransform, Read<TTransform>, Read<Parent>>.Filter<None<Child, Static>> qRoots,
        Query<GlobalTransform, Read<TTransform>>.Filter<(Any<Parent, Child>, None<Static>)> qTreeTransforms,
        Query<GlobalTransform, Read<TTransform>>.Filter<None<Parent, Child, Static>> qOrphans
    )
    {
        qOrphans.ForEach(static (ref globalTransform, ref transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
        });

        qTreeTransforms.ForEach(static (ref globalTransform, ref transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
        });

        qRoots.ForEach(query, static (in query, in root, ref _, ref _, ref _) =>
        {
            TransformPropagation.Propagate(root, query, Mat4f.Identity());
        });
    }

    [System(CoreStage.PostUpdate)]
    static void HandleStatic(
        Commands commands,
        Query query,
        Query<GlobalTransform, Read<TTransform>, Read<Parent>>.Filter<(All<Static>, None<Child, StaticCalculated>)> qRoots,
        Query<GlobalTransform, Read<TTransform>>.Filter<(All<Static>, Any<Parent, Child>, None<StaticCalculated>)> qTreeTransforms,
        Query<GlobalTransform, Read<TTransform>>.Filter<(All<Static>, None<Parent, Child, StaticCalculated>)> qOrphans
    )
    {
        qOrphans.ForEach((query, commands), static (in data, in entity, ref globalTransform, ref transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
            data.commands.AddComponent(entity, new StaticCalculated());
        });

        qTreeTransforms.ForEach((query, commands), static (in data, in entity, ref globalTransform, ref transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
            data.commands.AddComponent(entity, new StaticCalculated());
        });

        qRoots.ForEach(query, static (in query, in root, ref _, ref _, ref _) =>
        {
            TransformPropagation.Propagate(root, query, Mat4f.Identity());
        });
    }

    public void Apply(World world)
    {
        if (typeof(TTransform) == typeof(Transform2D))
        {
            world.Schedule.AddSystemSet<Transform2DSystems>();
            return;
        }

        world.Schedule.AddSystemSet<TransformPlugin<TTransform>>();
    }
}

[SystemSet]
public partial class Transform2DSystems
{
    readonly struct UpdateLocalTransforms : IChunkForEach<GlobalTransform, Read<Transform2D>>
    {
        public void Execute(
            scoped in ReadOnlySpan<Entity> entities,
            in Span<GlobalTransform> globalTransforms,
            in Span<Read<Transform2D>> transforms
        )
        {
            for (var i = 0; i < globalTransforms.Length; i++)
            {
                globalTransforms[i].Value = transforms[i].Component.ToMat4f();
            }
        }
    }

    [System(CoreStage.PostUpdate)]
    static void HandleDynamic(
        Query query,
        Query<GlobalTransform, Read<Transform2D>, Read<Parent>>.Filter<None<Child, Static>> qRoots,
        Query<GlobalTransform, Read<Transform2D>>.Filter<(Any<Parent, Child>, None<Static>)> qTreeTransforms,
        Query<GlobalTransform, Read<Transform2D>>.Filter<None<Parent, Child, Static>> qOrphans
    )
    {
        qOrphans.ForEachChunk(new UpdateLocalTransforms());
        qTreeTransforms.ForEachChunk(new UpdateLocalTransforms());

        qRoots.ForEach(query, static (in query, in root, ref _, ref _, ref _) =>
        {
            TransformPropagation.Propagate(root, query, Mat4f.Identity());
        });
    }

    [System(CoreStage.PostUpdate)]
    static void HandleStatic(
        Commands commands,
        Query query,
        Query<GlobalTransform, Read<Transform2D>, Read<Parent>>.Filter<(All<Static>, None<Child, StaticCalculated>)> qRoots,
        Query<GlobalTransform, Read<Transform2D>>.Filter<(All<Static>, Any<Parent, Child>, None<StaticCalculated>)> qTreeTransforms,
        Query<GlobalTransform, Read<Transform2D>>.Filter<(All<Static>, None<Parent, Child, StaticCalculated>)> qOrphans
    )
    {
        qOrphans.ForEach((query, commands), static (in data, in entity, ref globalTransform, ref transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
            data.commands.AddComponent(entity, new StaticCalculated());
        });

        qTreeTransforms.ForEach((query, commands), static (in data, in entity, ref globalTransform, ref transform) =>
        {
            globalTransform.Value = transform.Component.ToMat4f();
            data.commands.AddComponent(entity, new StaticCalculated());
        });

        qRoots.ForEach(query, static (in query, in root, ref _, ref _, ref _) =>
        {
            TransformPropagation.Propagate(root, query, Mat4f.Identity());
        });
    }
}

static class TransformPropagation
{
    public static void Propagate(in Entity current, in Query query, in Mat4f parentTransform)
    {
        ref var child = ref query.TryGet<Child>(current, out var hasChild);
        if (hasChild && child.NextSibling != Entity.Null)
        {
            Propagate(child.NextSibling, query, parentTransform);
        }

        ref var globalTransform = ref query.TryGet<GlobalTransform>(current, out var hasGlobalTransform);
        if (!hasGlobalTransform)
        {
            return;
        }

        globalTransform.Value = parentTransform * globalTransform.Value;

        ref var parent = ref query.TryGet<Parent>(current, out var hasParent);
        if (hasParent)
        {
            Propagate(parent.FirstChild, query, globalTransform.Value);
        }
    }
}
