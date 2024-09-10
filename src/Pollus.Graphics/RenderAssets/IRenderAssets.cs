namespace Pollus.Graphics;

using Pollus.Utils;

public interface IRenderAssets
{
    public IRenderAssets Add<T>(Handle handle, T data) where T : notnull;
    public Handle<T> Add<T>(T data) where T : notnull;
    public object Get(Handle handle);
    public TRenderData Get<TRenderData>(Handle handle) where TRenderData : notnull;
}