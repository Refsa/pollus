namespace Pollus.Generators;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class TweenGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Pollus.Engine.Tween.TweenAttribute",
            predicate: static (syntaxNode, cancellationToken) =>
                (syntaxNode is StructDeclarationSyntax structDecl && structDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                || (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword) && recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)),
            transform: Common.CollectType
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var partialExt =
                $$"""
                  {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.FullClassName}} : Pollus.Engine.Tween.ITweenable
                  {
                      {{GetTweenImpl(model)}}
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
                  using System.Runtime.CompilerServices;
                  using System.Linq.Expressions;
                  using System.Reflection;
                  using Pollus.Engine.Tween;
                  using Pollus.ECS;

                  {{partialExt}}
                  """, Encoding.UTF8);

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.FileName ?? "root"}_{model.TypeInfo.FileName}.Tweenable.gen.cs", source);
        });
    }

    internal static string GetTweenImpl(Model model)
    {
        var distinctFieldTypes = new HashSet<string>(model.Fields.Select(e => e.Type));

        return
            $$"""
                  public static void PrepareTweenSystems(Schedule schedule)
                  {
              {{string.Join("\n", distinctFieldTypes.Select(e => $"        schedule.AddSystems(CoreStage.Update, new TweenSystem<{model.TypeInfo.FullClassName}, {e}>());"))}}
                  }
              """;
    }
}