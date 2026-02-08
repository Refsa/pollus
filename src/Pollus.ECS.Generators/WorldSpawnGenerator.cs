namespace Pollus.ECS.Generators
{
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class WorldSpawnGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(GenerateWorldSpawnExtensions);
        }

        void GenerateWorldSpawnExtensions(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

public static class WorldSpawnExtensions
{
    $methods$
}";

            const string METHOD_TEMPLATE =
@"
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Entity Spawn<$gen_args$>(this World world, $parameters$)
        $gen_constraints$
    {
        var builder = new EntityBuilder<$gen_args$>()
        {
            $set_components$
        };
        return builder.Spawn(world);
    }";

            var sb = new StringBuilder(TEMPLATE);
            var methods = new StringBuilder();
            var gen_args = "C0, ";
            var gen_constraints = "where C0 : unmanaged, IComponent\n";
            var parameters = "in C0 c0, ";
            var set_components = "Component0 = c0,\n";

            for (int i = 1; i < 15; i++)
            {
                gen_args += $"C{i}";
                gen_constraints += $"where C{i} : unmanaged, IComponent";
                parameters += $"in C{i} c{i}";
                set_components += $"Component{i} = c{i},";

                methods.AppendLine(METHOD_TEMPLATE
                    .Replace("$gen_args$", gen_args)
                    .Replace("$parameters$", parameters)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$set_components$", set_components)
                );

                gen_args += ", ";
                gen_constraints += "\n";
                parameters += ", ";
                set_components += "\n";
            }

            sb.Replace("$methods$", methods.ToString());
            context.AddSource("WorldSpawnExtensions.gen.cs", sb.ToString());
        }
    }
}