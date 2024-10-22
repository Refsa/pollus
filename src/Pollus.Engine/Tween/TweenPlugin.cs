namespace Pollus.Engine.Tween;

using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Pollus.ECS;
using Pollus.Mathematics;

public class TweenPlugin : IPlugin
{
    static TweenPlugin()
    {
        TweenResources.RegisterHandler<float, FloatHandler>();
        TweenResources.RegisterHandler<Vec2f, Vec2fHandler>();
    }

    public void Apply(World world)
    {
        world.Resources.Add(new TweenResources());
    }
}


public class TweenablePlugin<TData> : IPlugin
    where TData : unmanaged, IComponent, ITweenable<TData>
{
    public void Apply(World world)
    {
        TData.PrepareSystems(world.Schedule);
    }
}

public class TweenSystem<TData, TField> : SystemBase<Commands, Time, Query<TData, Tween<TField>>>
    where TData : unmanaged, IComponent, ITweenable<TData>
    where TField : unmanaged
{
    public delegate void ApplyDelegate(TData data, TField value);

    public TweenSystem() : base(new($"TweenSystem<{typeof(TData).Name}, {typeof(TField).Name}>"))
    {
    }

    protected override void OnTick(Commands commands, Time time, Query<TData, Tween<TField>> qTweens)
    {
        var handler = TweenResources.Handler<TField>.Instance;

        qTweens.ForEach((commands, time, handler!),
        static (in (Commands commands, Time time, ITweenHandler<TField> handler) userData,
                in Entity entity, ref TData data, ref Tween<TField> tween) =>
        {
            var t = tween.Elapsed / tween.Duration;
            t = tween.Easing switch
            {
                Easing.Linear => t,
                Easing.Sine => float.Sin(t),
                Easing.Quadratic => t * t,
                Easing.Cubic => t * t * t,
                Easing.Quartic => t * t * t * t,
                Easing.Quintic => t * t * t * t * t,
                _ => t,
            };

            t = t.Clamp(0f, 1f);

            if (tween.Flags.HasFlag(TweenFlag.Reverse)) t = 1f - t;

            var value = userData.handler.Lerp(tween.From, tween.To, t);
            data.SetValue(tween.FieldID, value);

            if (t >= 1f)
            {
                if (tween.Flags.HasFlag(TweenFlag.Loop))
                {
                    tween.Elapsed = 0f;
                }
                else if (tween.Flags.HasFlag(TweenFlag.PingPong))
                {
                    (tween.From, tween.To) = (tween.To, tween.From);
                    tween.Elapsed = 0f;
                }
                else
                {
                    userData.commands.RemoveComponent<Tween<TField>>(entity);
                }
            }
            else
            {
                tween.Elapsed += userData.time.DeltaTimeF;
            }
        });
    }
}

class TweenResources
{
    public static class Handler<TType>
        where TType : unmanaged
    {
        static ITweenHandler<TType>? handler;
        public static ITweenHandler<TType>? Instance => handler;

        public static void Set(ITweenHandler<TType> handler) => Handler<TType>.handler = handler;
    }

    public static void RegisterHandler<TType, THandler>()
        where TType : unmanaged
        where THandler : struct, ITweenHandler<TType>
    {
        Handler<TType>.Set(new THandler());
    }
}

/* -------------------------------------------------------------------------------------------- */

public enum Easing : byte
{
    Linear = 0,
    Sine,
    Quadratic,
    Cubic,
    Quartic,
    Quintic,
}

public enum EasingDirection : byte
{
    InOut = 0,
    In,
    Out,
}

/* -------------------------------------------------------------------------------------------- */

[Flags]
public enum TweenFlag
{
    None = 0,
    Loop = 1 << 0,
    PingPong = 1 << 1,
    Reverse = 1 << 2,
    Delayed = 1 << 3,
}

public static class Tween
{
    public static TweenBuilder<TType> Create<TType>(float duration, TType from, TType to)
        where TType : unmanaged
    {
        return new TweenBuilder<TType>(from, to, duration);
    }
}

public struct TweenBuilder<TType>
    where TType : unmanaged
{
    Tween<TType> tween;
    Entity entity;

    public TweenBuilder(TType from, TType to, float duration)
    {
        tween = new Tween<TType>()
        {
            From = from,
            To = to,
            Duration = duration,
            Easing = Easing.Linear,
            Direction = EasingDirection.InOut,
        };
    }

    public TweenBuilder<TType> WithEasing(Easing easing)
    {
        tween.Easing = easing;
        return this;
    }

    public TweenBuilder<TType> WithDirection(EasingDirection direction)
    {
        tween.Direction = direction;
        return this;
    }

    public TweenBuilder<TType> OnField<TData>(Expression<Func<TData, TType>> property)
        where TData : unmanaged, ITweenable<TData>
    {
        tween.FieldID = TData.GetFieldIndex(property);
        return this;
    }

    public TweenBuilder<TType> OnEntity(Entity entity)
    {
        this.entity = entity;
        return this;
    }

    public TweenBuilder<TType> WithFlags(TweenFlag flags)
    {
        tween.Flags = flags;
        return this;
    }

    public void Append(Commands commands)
    {
        commands.AddComponent(entity, tween);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Tween<TType> : IComponent
    where TType : unmanaged
{
    public Easing Easing;
    public EasingDirection Direction;
    public TweenFlag Flags;

    public byte FieldID;

    public float Duration;
    public float Elapsed;

    public TType From;
    public TType To;
}

/* -------------------------------------------------------------------------------------------- */

public interface ITweenHandler<TData>
    where TData : unmanaged
{
    TData Lerp(TData from, TData to, float t);
}

public struct FloatHandler : ITweenHandler<float>
{
    public float Lerp(float from, float to, float t) => float.Lerp(from, to, t);
}

public struct Vec2fHandler : ITweenHandler<Vec2f>
{
    public Vec2f Lerp(Vec2f from, Vec2f to, float t) => Vec2f.Lerp(from, to, t);
}

/* -------------------------------------------------------------------------------------------- */

public struct TweenCommand : ICommand
{
    public static int Priority => 99;

    public Entity Entity;

    public void Execute(World world)
    {

    }
}

public static class TweenCommandsExt
{
    public static Commands Tween(this Commands commands, Entity entity)
    {
        return commands;
    }
}

/* -------------------------------------------------------------------------------------------- */

[AttributeUsage(AttributeTargets.Struct)]
public sealed class TweenableAttribute : Attribute { }

public interface ITweenable
{
    void SetValue<T>(byte field, T value);
    static abstract void PrepareSystems(Schedule schedule);
}

public interface ITweenable<TData> : ITweenable
    where TData : unmanaged
{
    static abstract byte GetFieldIndex<TField>(Expression<Func<TData, TField>> property);
}