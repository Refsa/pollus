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
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Entity Spawn<$gen_args$>(this World world, $parameters$)
        $gen_constraints$
    {
        var entityRef = world.Archetypes.CreateEntity<EntityBuilder<$gen_args$>>();
        ref var chunk = ref entityRef.Archetype.GetChunk(entityRef.ChunkIndex);
        $set_components$
        return entityRef.Entity;
    }";

            var sb = new StringBuilder(TEMPLATE);
            var methods = new StringBuilder();
            var gen_args = "C0, ";
            var gen_constraints = "where C0 : unmanaged, IComponent\n";
            var parameters = "in C0 c0, ";
            var set_components = "chunk.SetComponent(entityRef.RowIndex, c0);\n";

            for (int i = 1; i < 15; i++)
            {
                gen_args += $"C{i}";
                gen_constraints += $"where C{i} : unmanaged, IComponent";
                parameters += $"in C{i} c{i}";
                set_components += $"chunk.SetComponent(entityRef.RowIndex, c{i});";

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