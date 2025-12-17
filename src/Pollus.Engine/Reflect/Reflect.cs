namespace Pollus.Engine.Reflect;

using System.Linq.Expressions;
using Pollus.ECS;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class ReflectAttribute : Attribute;

public interface IReflect
{
    static abstract byte[] Fields { get; }
    void SetValue<T>(byte field, T value);
}

public interface IReflect<TData> : IReflect
    where TData : unmanaged
{
    static abstract byte GetFieldIndex(string fieldName);
    static abstract byte GetFieldIndex<TField>(Expression<Func<TData, TField>> property);
}