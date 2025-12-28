namespace Pollus.Generators;

using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class AssetGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Pollus.Core.Assets.AssetAttribute",
            predicate: static (syntaxNode, cancellationToken) =>
                (syntaxNode is StructDeclarationSyntax structDecl && structDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                || (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                || (syntaxNode is ClassDeclarationSyntax classDecl && classDecl.Modifiers.Any(SyntaxKind.PartialKeyword)),
            transform: Common.CollectType
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var partialExt =
                $$"""
                  {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.FullClassName}} 
                    : Pollus.Core.Assets.IAsset
                  {
                      {{GetAssetImpl(model)}}
                  } 
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
                  using Pollus.Core.Assets;
                  using Pollus.Utils;
                  using System.Collections.Generic;

                  {{partialExt}}
                  """, Encoding.UTF8);

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.FileName ?? "root"}_{model.TypeInfo.FileName}.Asset.gen.cs", source);
        });
    }

    internal static string GetAssetImpl(Model model)
    {
        var assetFields = model.Fields
            .Where(e => e.TypeAttributes.Contains("Pollus.Core.Assets.AssetAttribute"))
            .Select(e => $"..{e.Name}.Dependencies");
        var handleFields = model.Fields
            .Where(e => e.FullClassName.StartsWith("global::Pollus.Utils.Handle"))
            .Select(e => e.Name);

        string[] dependencies = [..handleFields, ..assetFields];

        if (!dependencies.Any())
        {
            return "public HashSet<Handle> Dependencies => [];";
        }

        return
            $$"""
                  public HashSet<Handle> Dependencies => [
                    {{string.Join(",\n\t\t", dependencies)}}
                  ];
              """;
    }
}