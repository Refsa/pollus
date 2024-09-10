namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Utils;

public readonly record struct FramePassHandle(int PassIndex)
{
    public static implicit operator FramePassHandle(int passIndex) => new(passIndex);
}

public interface IFramePass
{
}

public struct FramePass<TParam, TData> : IFramePass
    where TData : struct
{
    public TData Data;
    public FrameGraph<TParam>.ExecuteDelegate<TData> Execute;

    public FramePass(TData data, FrameGraph<TParam>.ExecuteDelegate<TData> execute)
    {
        Data = data;
        Execute = execute;
    }
}

public interface IFramePassContainer<TParam>
{
    void Clear();
    void Execute(RenderContext context, TParam renderAssets);
}

public class FramePassContainer<TExecuteParam, TData> : IFramePassContainer<TExecuteParam>
    where TData : struct
{
    FramePass<TExecuteParam, TData> pass;

    public void Set(in TData data, FrameGraph<TExecuteParam>.ExecuteDelegate<TData> execute)
    {
        pass = new FramePass<TExecuteParam, TData>(data, execute);
    }

    public ref FramePass<TExecuteParam, TData> Get()
    {
        return ref pass;
    }

    public void Clear()
    {
        pass = default;
        Pool<FramePassContainer<TExecuteParam, TData>>.Shared.Return(this);
    }

    public void Execute(RenderContext context, TExecuteParam renderAssets)
    {
        pass.Execute(context, renderAssets, pass.Data);
    }
}

public struct FramePassContainer<TParam> : IDisposable
{
    List<IFramePassContainer<TParam>> containers;
    Dictionary<Type, int> containerLookup;

    public FramePassContainer()
    {
        containers = Pool<List<IFramePassContainer<TParam>>>.Shared.Rent();
        containerLookup = Pool<Dictionary<Type, int>>.Shared.Rent();
    }

    public void Dispose()
    {
        for (int i = 0; i < containers.Count; i++) containers[i].Clear();
        containers.Clear();
        containerLookup.Clear();
        
        Pool<List<IFramePassContainer<TParam>>>.Shared.Return(containers);
        Pool<Dictionary<Type, int>>.Shared.Return(containerLookup);
    }

    public FramePassHandle AddPass<TData>(in TData data, FrameGraph<TParam>.ExecuteDelegate<TData> execute)
        where TData : struct
    {
        if (containerLookup.ContainsKey(typeof(TData)))
        {
            throw new Exception($"Pass of type {typeof(TData)} already exists");
        }

        // TODO: recycle
        var container = Pool<FramePassContainer<TParam, TData>>.Shared.Rent();

        var handle = new FramePassHandle(containers.Count);
        containers.Add(container);
        containerLookup.Add(typeof(TData), handle.PassIndex);
        container.Set(data, execute);
        return handle;
    }

    public void ExecutePass(FramePassHandle handle, RenderContext renderContext, TParam param)
    {
        containers[handle.PassIndex].Execute(renderContext, param);
    }

    public IFramePassContainer<TParam> GetPass(in FramePassHandle handle)
    {
        return containers[handle.PassIndex];
    }

    public ref FramePass<TParam, TData> GetPass<TData>()
        where TData : struct
    {
        var container = (FramePassContainer<TParam, TData>)containers[containerLookup[typeof(TData)]];
        return ref container.Get();
    }

    public ref FramePass<TParam, TData> GetPass<TData>(in FramePassHandle handle)
        where TData : struct
    {
        var container = (FramePassContainer<TParam, TData>)containers[handle.PassIndex];
        return ref container.Get();
    }
}