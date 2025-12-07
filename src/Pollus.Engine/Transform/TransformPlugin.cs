namespace Pollus.Engine.Transform;

using Pollus.Debugging;
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
    [System(nameof(HandleDynamic))] static readonly SystemBuilderDescriptor HandleDynamicSystemDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
    };

    [System(nameof(HandleStatic))] static readonly SystemBuilderDescriptor HandleStaticSystemDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
    };

    static void HandleDynamic(
        Commands commands,
        Query query,
        Query<GlobalTransform, Read<TTransform>, Read<Parent>>.Filter<None<Child, Static>> qRoots,
        Query<GlobalTransform, Read<TTransform>>.Filter<(Any<Parent, Child>, None<Static>)> qTreeTransforms,
        Query<GlobalTransform, Read<TTransform>>.Filter<None<Parent, Child, Static>> qOrphans
    )
    {
        qOrphans.ForEach(static (ref globalTransform, ref transform) => { globalTransform.Value = transform.Component.ToMat4f(); });

        qTreeTransforms.ForEach(static (ref globalTransform, ref transform) => { globalTransform.Value = transform.Component.ToMat4f(); });

        qRoots.ForEach(query, static (in query, in root, ref globalTransform, ref transform, ref parent) => { Propagate(root, query, Mat4f.Identity()); });
    }

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

        qRoots.ForEach(query, static (in query, in root, ref globalTransform, ref transform, ref parent) => { Propagate(root, query, Mat4f.Identity()); });
    }

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

    public void Apply(World world)
    {
        world.Schedule.AddSystemSet<TransformPlugin<TTransform>>();
    }
}
