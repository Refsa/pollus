namespace Pollus.ECS.Generators;

using System;
using System.Text;
using Microsoft.CodeAnalysis;

[Generator]
public class SystemParamGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(GenerateParams);
    }

    void GenerateParams(IncrementalGeneratorPostInitializationContext context)
    {
        const string TEMPLATE = @"
namespace Pollus.ECS;
using Pollus.ECS.Core;
using System.Runtime.CompilerServices;

public struct Param<$gen_args$> : IFetch<Param<$gen_args$>>, ISystemParam
{
    static readonly HashSet<Type> dependencies;
    public static HashSet<Type> Dependencies => dependencies;

$fetch_infos$

    static Param()
    {
#pragma warning disable IL2059
$run_static_constructors$
#pragma warning restore IL2059
$assign_fetch_infos$
        dependencies = [$dependencies$];

        Register();
    }

    public static void Register()
    {
        Fetch.Register<Param<$gen_args$>>(new Param<$gen_args$>(), [.. dependencies]);
    }

$param_fields$

    public Param<$gen_args$> DoFetch(World world, ISystem system)
    {
        return new Param<$gen_args$> { 
$do_fetches$
        };
    }
}
        ";

        var sb = new StringBuilder();

        var gen_args = "T0";
        var fetch_infos = "\tstatic readonly Fetch.Info t0Fetch;";
        var run_static_constructors = "\t\tRuntimeHelpers.RunClassConstructor(typeof(T0).TypeHandle);";
        var assign_fetch_infos = "\t\tt0Fetch = Fetch.Get<T0>();";
        var dependencies = ".. t0Fetch.Dependencies";
        var param_fields = "\tpublic T0 Param0;";
        var do_fetches = "\t\t\tParam0 = ((IFetch<T0>)t0Fetch.Fetch).DoFetch(world, system),";

        for (int i = 1; i < 16; i++)
        {
            gen_args += $", T{i}";
            fetch_infos += $"\n\tstatic readonly Fetch.Info t{i}Fetch;";
            run_static_constructors += $"\n\t\tRuntimeHelpers.RunClassConstructor(typeof(T{i}).TypeHandle);";
            assign_fetch_infos += $"\n\t\tt{i}Fetch = Fetch.Get<T{i}>();";
            dependencies += $", .. t{i}Fetch.Dependencies";
            param_fields += $"\n\tpublic T{i} Param{i};";
            do_fetches += $"\n\t\t\tParam{i} = ((IFetch<T{i}>)t{i}Fetch.Fetch).DoFetch(world, system),";

            context.AddSource($"SystemParams{i}.gen.cs", TEMPLATE
                .Replace("$gen_args$", gen_args)
                .Replace("$fetch_infos$", fetch_infos)
                .Replace("$run_static_constructors$", run_static_constructors)
                .Replace("$assign_fetch_infos$", assign_fetch_infos)
                .Replace("$dependencies$", dependencies)
                .Replace("$param_fields$", param_fields)
                .Replace("$do_fetches$", do_fetches));
        }

    }
}