namespace Pollus.Engine.Tween;

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.ECS;
using Pollus.Mathematics;

public class TweenPlugin : IPlugin
{
    public void Apply(World world)
    {
        TweenResources.RegisterHandler<float, FloatHandler>();

        world.Resources.Add(new TweenResources());

        world.Schedule.AddSystems(CoreStage.Update, new TweenSystem<TweenTestComponent, float>());
    }
}

class TweenSystem<TData, TField> : SystemBase<Commands, Time, Query<TData, Tween<TField>>>
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

            if (t >= 1f)
            {
                t = 1f;
                userData.commands.RemoveComponent<Tween<TField>>(entity);
            }

            var value = userData.handler.Lerp(tween.From, tween.To, t);
            data.SetValue(tween.FieldID, value);

            tween.Elapsed += userData.time.DeltaTimeF;
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

public interface ITweenable
{
    void SetValue<T>(byte field, T value);
}

public interface ITweenable<TData> : ITweenable
    where TData : unmanaged
{
    static abstract byte GetFieldIndex<TField>(Expression<Func<TData, TField>> property);
}

public partial struct TweenTestComponent : IComponent, ITweenable<TweenTestComponent>
{
    public float Float;
    public Vec2f Vec2f;

    public enum TweenField : byte
    {
        Float,
        Vec2f,
    }

    public void SetValue<T>(byte field, T value) => SetValue((TweenField)field, value);
    public void SetValue<T>(TweenField field, T value)
    {
        switch (field)
        {
            case TweenField.Float: Float = Unsafe.As<T, float>(ref value); break;
            case TweenField.Vec2f: Vec2f = Unsafe.As<T, Vec2f>(ref value); break;
            default: throw new ArgumentException($"Invalid property: {field}", nameof(field));
        }
    }

    public static byte GetFieldIndex<TField>(Expression<Func<TweenTestComponent, TField>> property)
    {
        string? fieldName = null;
        if (property.Body is MemberExpression expr)
        {
            fieldName = (expr.Member as FieldInfo)?.Name;
        }

        if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException("Invalid property expression", nameof(property));
        return (byte)Enum.Parse<TweenField>(fieldName);
    }
}