namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public readonly record struct FramePassHandle(int PassIndex)
{
    public static implicit operator FramePassHandle(int passIndex) => new(passIndex);
}

public interface IFramePass
{
}

public struct FramePass<TExecuteParam, TData> : IFramePass
    where TData : struct
{
    public TData Data;
    public FrameGraph<TExecuteParam>.ExecuteDelegate<TData> Execute;

    public FramePass(TData data, FrameGraph<TExecuteParam>.ExecuteDelegate<TData> execute)
    {
        Data = data;
        Execute = execute;
    }
}

public interface IFramePassContainer<TExecuteParam>
{
    void Clear();
    void Execute(RenderContext context, TExecuteParam renderAssets);
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
    }

    public void Execute(RenderContext context, TExecuteParam renderAssets)
    {
        pass.Execute(context, renderAssets, pass.Data);
    }
}

public class FramePassContainer<TExecuteParam>
{
    List<IFramePassContainer<TExecuteParam>> containers = [];
    Dictionary<Type, int> containerLookup = [];

    public FramePassContainer()
    {
        containers = new List<IFramePassContainer<TExecuteParam>>();
        containerLookup = new Dictionary<Type, int>();
    }

    public void Clear()
    {
        // TODO: recycle
        containers.Clear();
        containerLookup.Clear();
    }

    public FramePassHandle AddPass<TData>(in TData data, FrameGraph<TExecuteParam>.ExecuteDelegate<TData> execute)
        where TData : struct
    {
        if (containerLookup.ContainsKey(typeof(TData)))
        {
            throw new Exception($"Pass of type {typeof(TData)} already exists");
        }

        // TODO: recycle
        var container = new FramePassContainer<TExecuteParam, TData>();
        var handle = new FramePassHandle(containers.Count);
        containers.Add(container);
        containerLookup.Add(typeof(TData), handle.PassIndex);
        container.Set(data, execute);
        return handle;
    }

    public void ExecutePass(FramePassHandle handle, RenderContext renderContext, TExecuteParam param)
    {
        containers[handle.PassIndex].Execute(renderContext, param);
    }

    public IFramePassContainer<TExecuteParam> GetPass(in FramePassHandle handle)
    {
        return containers[handle.PassIndex];
    }

    public ref FramePass<TExecuteParam, TData> GetPass<TData>()
        where TData : struct
    {
        var container = (FramePassContainer<TExecuteParam, TData>)containers[containerLookup[typeof(TData)]];
        return ref container.Get();
    }

    public ref FramePass<TExecuteParam, TData> GetPass<TData>(in FramePassHandle handle)
        where TData : struct
    {
        var container = (FramePassContainer<TExecuteParam, TData>)containers[handle.PassIndex];
        return ref container.Get();
    }
}