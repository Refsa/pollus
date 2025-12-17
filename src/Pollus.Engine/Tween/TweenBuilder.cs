namespace Pollus.Engine.Tween;

using System.Linq.Expressions;
using Pollus.ECS;
using Pollus.Utils;

public static class Tween
{
    public static TweenBuilder<TType, TField> Create<TType, TField>(Entity entity, Expression<Func<TType, TField>> property)
        where TType : unmanaged, IReflect<TType>, ITweenable
        where TField : unmanaged
    {
        return new TweenBuilder<TType, TField>(entity, property);
    }

    public static TweenSequenceBuilder Sequence(Commands commands)
    {
        return new TweenSequenceBuilder(commands);
    }
}

public struct TweenBuilder<TType, TField>
    where TType : unmanaged, IReflect<TType>, ITweenable
    where TField : unmanaged
{
    Tween<TField> tween;
    TweenData data;
    Entity entity;

    public TweenBuilder(Entity entity, Expression<Func<TType, TField>> property)
    {
        this.entity = entity;
        this.tween = new()
        {
            FieldID = TType.GetFieldIndex(property),
        };
    }

    public TweenBuilder<TType, TField> WithFromTo(TField from, TField to)
    {
        tween.From = from;
        tween.To = to;
        return this;
    }

    public TweenBuilder<TType, TField> WithDuration(float duration)
    {
        data.Duration = duration;
        return this;
    }

    public TweenBuilder<TType, TField> WithEasing(Easing easing)
    {
        data.Easing = easing;
        return this;
    }

    public TweenBuilder<TType, TField> WithDirection(EasingDirection direction)
    {
        data.Direction = direction;
        return this;
    }

    public TweenBuilder<TType, TField> WithFlags(TweenFlag flags)
    {
        data.Flags = flags;
        return this;
    }

    public Entity Append(Commands commands, bool setParent = true)
    {
        var childCommands = commands.Spawn(
            Entity.With(tween).With(data).With(new TweenTarget() { Entity = entity })
        );

        if (setParent)
        {
            childCommands = childCommands.SetParent(entity);
        }

        return childCommands.Entity;
    }
}
