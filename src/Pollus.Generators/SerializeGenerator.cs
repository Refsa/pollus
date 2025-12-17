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
            var lookupType = model.TypeInfo.IsUnmanaged ? "BlittableSerializer" : "Serializer";

            var partialExt =
                $$"""
                  {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.ClassName}} : Pollus.Core.Serialization.ISerializable
                  {
                      {{GetISerializableImpl(model)}}

                      [ModuleInitializer]
                      public static void {{model.TypeInfo.ClassName}}Serializer_ModuleInitializer()
                      {
                          {{lookupType}}Lookup.RegisterSerializer(new {{model.TypeInfo.ClassName}}Serializer());
                      }
                  } 

                  {{GenerateSerializerImpl(model)}}
                  """;

            if (model.ContainingType != null)
            {
                partialExt =
                    $$"""
                      {{model.ContainingType.Visibility}} partial {{model.ContainingType.FullTypeKind}} {{model.ContainingType.ClassName}}
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

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.ClassName ?? "root"}_{model.TypeInfo.ClassName}.Serialize.gen.cs", source);
        });
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

        return
            $$"""
              public class {{model.TypeInfo.ClassName}}Serializer : {{serializerType}}<{{model.TypeInfo.ClassName}}>
              {
                  public {{model.TypeInfo.ClassName}} Deserialize<TReader>(ref TReader reader) where TReader : IReader
                  {
                      var c = new {{model.TypeInfo.ClassName}}();
                      c.Deserialize(ref reader);
                      return c;
                  }  
                  public void Serialize<TWriter>(ref TWriter writer, ref {{model.TypeInfo.ClassName}} value) where TWriter : IWriter
                  {
                      value.Serialize(ref writer);
                  }
              }
              """;
    }
}