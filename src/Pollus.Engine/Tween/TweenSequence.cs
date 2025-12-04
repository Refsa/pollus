namespace Pollus.Engine.Tween;

using Pollus.ECS;
using Pollus.Engine.Reflect;

public struct TweenSequence : IComponent
{
    public Entity Current;
    public TweenFlag Flags;
}

public struct TweenSequenceBuilder
{
    EntityCommands sequenceCommands;
    TweenSequence sequence;
    Commands commands;

    public TweenSequenceBuilder(Commands commands)
    {
        this.commands = commands;
        this.sequenceCommands = commands.Spawn();
        sequence = new()
        {
            Current = Entity.NULL,
            Flags = TweenFlag.None,
        };
    }

    public TweenSequenceBuilder WithFlags(TweenFlag flags)
    {
        sequence.Flags = flags;
        return this;
    }

    public TweenSequenceBuilder Then<TType, TField>(TweenBuilder<TType, TField> tween)
        where TType : unmanaged, IReflect<TType>, ITweenable
        where TField : unmanaged
    {
        var tweenEntity = tween.Append(commands, false);
        sequenceCommands = sequenceCommands.AddChild(tweenEntity);

        if (sequence.Current == Entity.NULL)
        {
            sequence.Current = tweenEntity;
        }
        else
        {
            commands.AddComponent(tweenEntity, new TweenDisabled());
        }

        return this;
    }

    public Entity Append()
    {
        return sequenceCommands.AddComponent(sequence).Entity;
    }
}

[SystemSet]
public partial class TweenSequenceSystemSet
{
    [System(nameof(SequenceSystem))]
    static readonly SystemBuilderDescriptor SequenceSystemDescriptor = new()
    {
        Stage = CoreStage.Last,
    };

    static void SequenceSystem(Commands commands, Time time, Query query, Query<TweenSequence, Read<Parent>> qSequences)
    {
        qSequences.ForEach((commands, query), static (in userData, in entity, ref sequence, ref parent) =>
        {
            ref var current = ref userData.query.Get<TweenData>(sequence.Current);
            if (current.Progress < 1f) return;

            ref var child = ref userData.query.Get<Child>(sequence.Current);
            if (child.NextSibling == Entity.NULL)
            {
                if (sequence.Flags.HasFlag(TweenFlag.OneShot))
                {
                    userData.commands.DespawnHierarchy(entity);
                }
                else if (sequence.Flags.HasFlag(TweenFlag.Loop))
                {
                    sequence.Current = parent.Component.FirstChild;
                    userData.commands.RemoveComponent<TweenDisabled>(sequence.Current);
                }
                else if (sequence.Flags.HasFlag(TweenFlag.PingPong))
                {
                    userData.commands.RemoveComponent<TweenDisabled>(sequence.Current);
                }

                return;
            }

            sequence.Current = child.NextSibling;
            userData.commands.RemoveComponent<TweenDisabled>(sequence.Current);
        });
    }
}

