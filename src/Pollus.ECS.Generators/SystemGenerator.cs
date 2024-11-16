namespace Pollus.ECS.Generators
{
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class SystemGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(GenerateSystems);
            context.RegisterPostInitializationOutput(GenerateSystemBuilders);
        }

        void GenerateSystemBuilders(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;

#pragma warning disable IL2059

public partial class FnSystem
{
$fn_system_methods$
}

#pragma warning restore IL2059
";

            var fn_system_methods = new StringBuilder();

            var gen_args = "T0";

            for (int i = 1; i < 16; i++)
            {
                gen_args += $", T{i}";

                fn_system_methods.AppendLine(@$"
    public static FnSystem<$gen_args$> Create<$gen_args$>(SystemBuilderDescriptor descriptor, SystemDelegate<$gen_args$> onTick)
    {{
        var system = new FnSystem<$gen_args$>(descriptor, onTick)
        {{
            RunCriteria = descriptor.RunCriteria
        }};
        if (descriptor.IsExclusive) system.Descriptor.DependsOn<ExclusiveSystemMarker>();
        foreach (var local in descriptor.Locals) system.Resources.Add(local, local.TypeID);
        return system;
    }}").Replace("$gen_args$", gen_args);
            }


            context.AddSource("SystemBuilder.gen.cs", TEMPLATE
                .Replace("$fn_system_methods$", fn_system_methods.ToString())
            );
        }

        void GenerateSystems(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

#pragma warning disable IL2059

public delegate void SystemDelegate<$gen_args$>($gen_params$);

public abstract class SystemBase<$gen_args$> : SystemBase
{
    static readonly HashSet<Type> dependencies;
    public static new HashSet<Type> Dependencies => dependencies;

    $fetch_fields$

    static SystemBase()
    {
        $run_class_ctor$
        $fetch_setups$
        dependencies = [$fetch_dependencies$];
    }

    public SystemBase(SystemDescriptor descriptor) : base(descriptor)
    {
        $set_parameter$
        descriptor.Dependencies.UnionWith(dependencies);
    }

    public override void Tick(World world)
    {   
        $do_fetch$
        OnTick($on_tick_params$);
    }

    protected override void OnTick() { }
    protected abstract void OnTick($gen_params$);
}

public class FnSystem<$gen_args$>(SystemDescriptor descriptor, SystemDelegate<$gen_args$> onTick) : SystemBase<$gen_args$>(descriptor)
{
    readonly SystemDelegate<$gen_args$> onTick = onTick;

    protected override void OnTick($gen_params$)
    {
        onTick($on_tick_params$);
    }
}

#pragma warning restore IL2059
";

            const string DO_FETCH_TEMPLATE = "var t$gen_idx$ = ((IFetch<$gen_arg$>)t$gen_idx$Fetch.Fetch).DoFetch(world, this);";
            const string FETCH_FIELDS_TEMPLATE = "static readonly Fetch.Info t$gen_idx$Fetch;";
            const string FETCH_SETUPS_TEMPLATE = "t$gen_idx$Fetch = Fetch.Get<$gen_arg$>();";
            const string FETCH_DEPENDENCIES_TEMPLATE = ".. t$gen_idx$Fetch.Dependencies";
            const string RUN_CLASS_CTOR_TEMPLATE = "RuntimeHelpers.RunClassConstructor(typeof($gen_arg$).TypeHandle);";


            var sb = new StringBuilder();
            var gen_args = "T0, ";
            var gen_params = "T0 t0, ";
            var on_tick_params = "t0, ";
            var do_fetch = DO_FETCH_TEMPLATE.Replace("$gen_idx$", "0").Replace("$gen_arg$", "T0");
            var fetch_fields = FETCH_FIELDS_TEMPLATE.Replace("$gen_idx$", "0");
            var fetch_setups = FETCH_SETUPS_TEMPLATE.Replace("$gen_idx$", "0").Replace("$gen_arg$", "T0");
            var fetch_dependencies = FETCH_DEPENDENCIES_TEMPLATE.Replace("$gen_idx$", "0");
            var run_class_ctor = RUN_CLASS_CTOR_TEMPLATE.Replace("$gen_arg$", "T0");
            var set_parameters = "descriptor.Parameters.Add(typeof(T0));";

            for (int i = 1; i < 16; i++)
            {
                gen_args += $"T{i}, ";
                gen_params += $"T{i} t{i}, ";
                on_tick_params += $"t{i}, ";
                do_fetch += DO_FETCH_TEMPLATE.Replace("$gen_idx$", i.ToString()).Replace("$gen_arg$", $"T{i}");
                set_parameters += $"\n        descriptor.Parameters.Add(typeof(T{i}));";
                fetch_fields += $"\n    static readonly Fetch.Info t{i}Fetch;";
                fetch_setups += $"\n        t{i}Fetch = Fetch.Get<T{i}>();";
                fetch_dependencies += $", ..t{i}Fetch.Dependencies";
                run_class_ctor += $"\n        RuntimeHelpers.RunClassConstructor(typeof(T{i}).TypeHandle);";

                sb.Clear().Append(TEMPLATE)
                    .Replace("$gen_idx$", i.ToString())
                    .Replace("$gen_arg$", $"T{i}")
                    .Replace("$gen_args$", gen_args.TrimEnd(' ', ','))
                    .Replace("$gen_params$", gen_params.TrimEnd(' ', ','))
                    .Replace("$on_tick_params$", on_tick_params.TrimEnd(' ', ','))
                    .Replace("$do_fetch$", do_fetch)
                    .Replace("$set_parameter$", set_parameters)
                    .Replace("$fetch_fields$", fetch_fields)
                    .Replace("$fetch_setups$", fetch_setups)
                    .Replace("$fetch_dependencies$", fetch_dependencies)
                    .Replace("$run_class_ctor$", run_class_ctor)
                    ;

                context.AddSource($"System{i + 1}.gen.cs", sb.ToString());
            }
        }
    }
}