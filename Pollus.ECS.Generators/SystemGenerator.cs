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
using Pollus.ECS.Core;

public partial class SystemBuilder
{
$methods$
}
";

            var sb = new StringBuilder();

            var gen_args = "T0";

            for (int i = 1; i < 16; i++)
            {
                gen_args += $", T{i}";

                sb.AppendLine(@$"
    public static SystemBuilder FnSystem<$gen_args$>(SystemLabel label, SystemDelegate<$gen_args$> onTick)
    {{
        return new SystemBuilder(new FnSystem<$gen_args$>(new SystemDescriptor(label), onTick));
    }}").Replace("$gen_args$", gen_args);
            }


            context.AddSource("SystemBuilder.gen.cs", TEMPLATE.Replace("$methods$", sb.ToString()));
        }

        void GenerateSystems(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS.Core;
using System.Runtime.CompilerServices;

public delegate void SystemDelegate<$gen_args$>($gen_params$);

public abstract class Sys<$gen_args$> : Sys
{
    static readonly HashSet<Type> dependencies;
    public static new HashSet<Type> Dependencies => dependencies;

    $fetch_fields$

    static Sys()
    {
#pragma warning disable IL2059
        $run_class_ctor$
#pragma warning restore IL2059
        $fetch_setups$
        dependencies = [$fetch_dependencies$];
    }

    public Sys(SystemDescriptor descriptor) : base(descriptor)
    {
        $depends_on$
    }

    public override void Tick(World world)
    {   
        $do_fetch$
        OnTick($on_tick_params$);
    }

    protected override void OnTick() { }
    protected abstract void OnTick($gen_params$);
}

public class FnSystem<$gen_args$>(SystemDescriptor descriptor, SystemDelegate<$gen_args$> onTick) : Sys<$gen_args$>(descriptor)
{
    readonly SystemDelegate<$gen_args$> onTick = onTick;

    protected override void OnTick($gen_params$)
    {
        onTick($on_tick_params$);
    }
}
";

            const string DO_FETCH_TEMPLATE = "var t$gen_idx$ = ((IFetch<$gen_arg$>)t$gen_idx$Fetch.Fetch).DoFetch(world, this);";
            const string DEPENDS_ON_TEMPLATE = "descriptor.DependsOn<$gen_arg$>();";
            const string FETCH_FIELDS_TEMPLATE = "static readonly Fetch.Info t$gen_idx$Fetch;";
            const string FETCH_SETUPS_TEMPLATE = "t$gen_idx$Fetch = Fetch.Get<$gen_arg$>();";
            const string FETCH_DEPENDENCIES_TEMPLATE = ".. t$gen_idx$Fetch.Dependencies";
            const string RUN_CLASS_CTOR_TEMPLATE = "RuntimeHelpers.RunClassConstructor(typeof($gen_arg$).TypeHandle);";


            var sb = new StringBuilder();
            var gen_args = "T0, ";
            var gen_params = "T0 t0, ";
            var on_tick_params = "t0, ";
            var do_fetch = DO_FETCH_TEMPLATE.Replace("$gen_idx$", "0").Replace("$gen_arg$", "T0");
            var depends_on = DEPENDS_ON_TEMPLATE.Replace("$gen_arg$", "T0");
            var fetch_fields = FETCH_FIELDS_TEMPLATE.Replace("$gen_idx$", "0");
            var fetch_setups = FETCH_SETUPS_TEMPLATE.Replace("$gen_idx$", "0").Replace("$gen_arg$", "T0");
            var fetch_dependencies = FETCH_DEPENDENCIES_TEMPLATE.Replace("$gen_idx$", "0");
            var run_class_ctor = RUN_CLASS_CTOR_TEMPLATE.Replace("$gen_arg$", "T0");

            for (int i = 1; i < 16; i++)
            {
                gen_args += $"T{i}, ";
                gen_params += $"T{i} t{i}, ";
                on_tick_params += $"t{i}, ";
                do_fetch += DO_FETCH_TEMPLATE.Replace("$gen_idx$", i.ToString()).Replace("$gen_arg$", $"T{i}");
                depends_on += DEPENDS_ON_TEMPLATE.Replace("$gen_arg$", $"T{i}");
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
                    .Replace("$depends_on$", depends_on)
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