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
                || (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword) && recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)),
            transform: Common.CollectType
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var partialExt =
                $$"""
                  {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.FullClassName}} : Pollus.Core.Serialization.ISerializable
                  {
                      {{GetISerializableImpl(model)}}

                      {{GetModuleInitializerImpl(model)}}
                  } 

                  {{GenerateSerializerImpl(model)}}
                  """;

            if (model.ContainingType != null)
            {
                partialExt =
                    $$"""
                      {{model.ContainingType.Visibility}} partial {{model.ContainingType.FullTypeKind}} {{model.ContainingType.FullClassName}}
                      {
                          {{partialExt}}
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

            var fullClassName = model.TypeInfo.IsGeneric ? $"{model.TypeInfo.ClassName}_{string.Join(", ", model.TypeInfo.GenericArguments.Select(e => e.ClassName))}" : model.TypeInfo.ClassName;
            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.ClassName ?? "root"}_{fullClassName}.Serialize.gen.cs", source);
        });
    }

    internal static string GetModuleInitializerImpl(Model model)
    {
        if (model.TypeInfo.IsGeneric) return null;

        var lookupType = model.TypeInfo.IsUnmanaged ? "BlittableSerializer" : "Serializer";
        return
            $$"""
                [ModuleInitializer]
                public static void {{model.TypeInfo.ClassName}}Serializer_ModuleInitializer()
                {
                    {{lookupType}}Lookup.RegisterSerializer(new {{model.TypeInfo.ClassName}}Serializer());
                }
              """;
    }

    internal static string GetISerializableImpl(Model model)
    {
        return
            $$"""
                public void Serialize<TWriter>(ref TWriter writer) where TWriter : IWriter
                {
                    {{string.Join("\n", model.Fields.Select(e => $"writer.Write({e.Name});"))}}
                }

                public void Deserialize<TReader>(ref TReader reader) where TReader : IReader
                {
                    {{string.Join("\n", model.Fields.Select(e => $"{e.Name} = reader.Read<{e.Type}>();"))}}
                }
              """;
    }

    internal static string GenerateSerializerImpl(Model model)
    {
        var serializerType = model.TypeInfo.IsUnmanaged ? "IBlittableSerializer" : "ISerializer";
        var genericArguments = model.TypeInfo.IsGeneric ? $"<{string.Join(", ", model.TypeInfo.GenericArguments.Select(e => e.ClassName))}>" : "";

        return
            $$"""
              public class {{model.TypeInfo.ClassName}}Serializer{{genericArguments}} : {{serializerType}}<{{model.TypeInfo.FullClassName}}>
              {
                  public {{model.TypeInfo.FullClassName}} Deserialize<TReader>(ref TReader reader) where TReader : IReader
                  {
                      var c = new {{model.TypeInfo.FullClassName}}();
                      c.Deserialize(ref reader);
                      return c;
                  }  
                  public void Serialize<TWriter>(ref TWriter writer, ref {{model.TypeInfo.FullClassName}} value) where TWriter : IWriter
                  {
                      value.Serialize(ref writer);
                  }
              }
              """;
    }
}