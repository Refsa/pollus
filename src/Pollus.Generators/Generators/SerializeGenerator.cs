namespace Pollus.Generators;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class SerializeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Pollus.Core.Serialization.SerializeAttribute",
            predicate: static (syntaxNode, cancellationToken) =>
                (syntaxNode is StructDeclarationSyntax structDecl && structDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                || (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                || (syntaxNode is ClassDeclarationSyntax classDecl && classDecl.Modifiers.Any(SyntaxKind.PartialKeyword)),
            transform: Common.CollectType
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            const string defaultContextType = "DefaultSerializationContext";

            var partialExt =
                $$"""
                  {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.FullClassName}} 
                    : Pollus.Core.Serialization.ISerializable<DefaultSerializationContext>
                  {
                      {{GetISerializableImpl(model, defaultContextType)}}
                      
                      {{(model.ContainingType is null ? GetModuleInitializerImpl(model, defaultContextType) : "")}}
                  } 

                  {{GetSerializerImpl(model, defaultContextType)}}
                  """;

            if (model.ContainingType != null)
            {
                partialExt =
                    $$"""
                      {{model.ContainingType.Visibility}} partial {{model.ContainingType.FullTypeKind}} {{model.ContainingType.FullClassName}}
                      {
                          {{partialExt}}

                          {{GetModuleInitializerImpl(model, defaultContextType)}}
                      }
                      """;
            }

            var source = SourceText.From(
                $$"""
                  namespace {{model.TypeInfo.Namespace}};
                  using System.Runtime.CompilerServices;
                  using System.Linq.Expressions;
                  using Pollus.Core.Serialization;

                  {{partialExt}}
                  """, Encoding.UTF8);

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.FileName ?? "root"}_{model.TypeInfo.FileName}.Serialize.gen.cs", source);
        });
    }

    private static IEnumerable<Field> GetFields(Model model)
    {
        return model.Fields.Where(e => !e.Attributes.Any(a => a.Contains("SerializeIgnore")));
    }

    internal static string GetModuleInitializerImpl(Model model, string contextType)
    {
        if (model.TypeInfo.IsGeneric) return null;

        var lookupType = model.TypeInfo.IsUnmanaged ? "BlittableSerializer" : "Serializer";
        // TODO: find alternative to [ModuleInitializer]
        return
            $$"""
                #pragma warning disable CA2255
                [ModuleInitializer]
                public static void {{model.TypeInfo.ClassName}}Serializer_ModuleInitializer()
                {
                    {{lookupType}}Lookup<{{contextType}}>.RegisterSerializer(new {{model.TypeInfo.ClassName}}Serializer());
                }
                #pragma warning restore CA2255
              """;
    }

    internal static string GetISerializableImpl(Model model, string contextType)
    {
        return
            $$"""
                public void Serialize<TWriter>(ref TWriter writer, in {{contextType}} context) where TWriter : IWriter, allows ref struct
                {
                    {{string.Join("\n", GetFields(model).Select(e => e.IsBlittable
                        ? $"writer.Write({e.Name}, \"{e.Name}\");"
                        : $"writer.Serialize({e.Name}, \"{e.Name}\");"
                    ))}}
                }

                public void Deserialize<TReader>(ref TReader reader, in {{contextType}} context) where TReader : IReader, allows ref struct
                {
                    {{string.Join("\n", GetFields(model).Select(e => e.IsBlittable
                        ? $"{e.Name} = reader.Read<{e.Type}>(\"{e.Name}\");"
                        : $"{e.Name} = reader.Deserialize<{e.Type}>(\"{e.Name}\");"
                    ))}}
                }
              """;
    }

    internal static string GetSerializerImpl(Model model, string contextType)
    {
        var serializerType = model.TypeInfo.IsUnmanaged ? "IBlittableSerializer" : "ISerializer";
        var genericArguments = model.TypeInfo.IsGeneric ? $"<{string.Join(", ", model.TypeInfo.GenericArguments.Select(e => e.TypeInfo.ClassName))}>" : "";
        var genericConstraints = model.TypeInfo.IsGeneric ? string.Join("\n", model.TypeInfo.GenericArguments.Select(e => $"where {e.TypeInfo.ClassName} : {string.Join(", ", e.Constraints)}")) : "";

        return
            $$"""
              {{model.TypeInfo.Visibility}} class {{model.TypeInfo.ClassName}}Serializer{{genericArguments}} : {{serializerType}}<{{model.TypeInfo.FullClassName}}, {{contextType}}>
              {{genericConstraints}}
              {
                  public {{model.TypeInfo.FullClassName}} Deserialize<TReader>(ref TReader reader, in {{contextType}} context) where TReader : IReader, allows ref struct
                  {
                      var c = new {{model.TypeInfo.FullClassName}}()
                      {
                          {{string.Join("\n", GetFields(model).Where(e => e.IsRequired).Select(e => $"{e.Name} = default!,"))}}
                      };
                      c.Deserialize(ref reader, in context);
                      return c;
                  }  
                  public void Serialize<TWriter>(ref TWriter writer, in {{model.TypeInfo.FullClassName}} value, in {{contextType}} context) where TWriter : IWriter, allows ref struct
                  {
                      value.Serialize(ref writer, in context);
                  }
              }
              """;
    }
}