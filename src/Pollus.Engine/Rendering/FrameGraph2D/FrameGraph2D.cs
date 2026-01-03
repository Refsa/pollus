namespace Pollus.Engine.Rendering;

using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;

public struct FrameGraph2DParam
{
    public required RenderAssets RenderAssets { get; init; }
    public required DrawGroups2D DrawGroups;
    public required Resources Resources;
    public required TextureFormat BackbufferFormat;
    public required Vec2<uint> BackbufferSize;
}

public struct PrepareTexturesPass
{
    public ResourceHandle<TextureResource> ColorAttachment;
}

public struct FinalBlitPass
{
    public ResourceHandle<TextureResource> ColorAttachment;
    public ResourceHandle<TextureResource> Backbuffer;
}

public class FrameGraph2D : IDisposable
{
    public static class Textures
    {
        public const string Backbuffer = "backbuffer";
        public const string ColorTarget = "color-target";
    }

    bool frameStarted;
    FrameGraph<FrameGraph2DParam> frameGraph;
    FrameGraph2DParam param;
    bool isDisposed;

    public ref readonly FrameGraph2DParam Param => ref param;
    public ref FrameGraph<FrameGraph2DParam> FrameGraph => ref frameGraph;
    public bool FrameStarted => frameStarted;

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);
        Cleanup();
    }

    public void BeginFrame(ref FrameGraph<FrameGraph2DParam> frameGraph, ref FrameGraph2DParam param)
    {
        frameStarted = true;
        this.frameGraph = frameGraph;
        this.param = param;
    }

    public FrameGraphRunner<FrameGraph2DParam> Compile()
    {
        if (!frameStarted)
            throw new InvalidOperationException("Frame not started");

        frameStarted = false;
        return frameGraph.Compile();
    }

    public void AddPass<TData>(RenderStep2D step, FrameGraph<FrameGraph2DParam>.BuilderDelegate<TData> builder, FrameGraph<FrameGraph2DParam>.ExecuteDelegate<TData> executor)
        where TData : struct
    {
        if (!frameStarted)
            throw new InvalidOperationException("Frame not started");

        frameGraph.AddPass(step, param, builder, executor);
    }

    public void Cleanup()
    {
        if (Unsafe.IsNullRef(ref frameGraph)) return;
        frameGraph.Dispose();
        Unsafe.SkipInit(out frameGraph);
    }
}