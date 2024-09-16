namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public interface IComputeShader
{
    public string Label { get; }
    public Handle<ShaderAsset> Shader { get; }
    public string EntryPoint { get; }
    IBinding[][] Bindings { get; }
}

public class ComputeShader : IComputeShader
{
    public required string Label { get; init; }
    public required Handle<ShaderAsset> Shader { get; init; }
    public required string EntryPoint { get; init; }
    public IBinding[][] Bindings { get; init; } = [];
}

public class ComputePlugin<TCompute> : IPlugin
    where TCompute : IComputeShader
{
    public const string SetupSystem = $"ComputePlugin<{nameof(TCompute)}>::Setup";

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new ComputeRenderDataLoader<TCompute>());

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(SetupSystem,
        static (IWGPUContext gpuContext, AssetServer assetServer, Assets<TCompute> computeShaders, RenderAssets renderAssets) =>
        {
            foreach (var info in computeShaders.AssetInfos)
            {
                renderAssets.Prepare(gpuContext, assetServer, info.Handle);
            }
        }));
    }
}