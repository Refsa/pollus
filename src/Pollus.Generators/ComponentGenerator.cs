namespace Pollus.Generators;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class ComponentGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (syntaxNode, cancellationToken) =>
            {
                if (syntaxNode is StructDeclarationSyntax structDecl)
                {
                    return structDecl.Modifiers.Any(SyntaxKind.PartialKeyword) && structDecl.BaseList is { Types.Count: > 0 };
                }

                if (syntaxNode is RecordDeclarationSyntax recordDecl)
                {
                    return recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) &&
                           recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword) &&
                           recordDecl.BaseList is { Types.Count: > 0 };
                }

                return false;
            },
            transform: static (context, cancellationToken) =>
            {
                var data = context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) as ITypeSymbol;
                if (data is null || !data.AllInterfaces.Any(i => i.ToDisplayString() == "Pollus.ECS.IComponent"))
                    return null;

                var fields = new List<Field>();
                Common.CollectFields(data, fields);

                TypeInfo containingType = null;
                if (data.ContainingType?.TypeKind is TypeKind.Struct or TypeKind.Class or TypeKind.Interface)
                {
                    var containingTypeKind = data.ContainingType.TypeKind.ToString().ToLower();
                    if (data.ContainingType.IsRecord && containingTypeKind != "record") containingTypeKind = $"record {containingTypeKind}";
                    containingType = new TypeInfo()
                    {
                        Namespace = data.ContainingNamespace.ToDisplayString(),
                        ClassName = data.ContainingType.Name,
                        FullTypeKind = containingTypeKind,
                        Visibility = data.ContainingType.DeclaredAccessibility.ToString().ToLower(),
                        Attributes = data.ContainingType.GetAttributes().Select(a => a.ToString()).ToArray(),
                    };
                }

                var fullTypeKind = data.TypeKind.ToString().ToLower();
                if (data.IsRecord && fullTypeKind != "record") fullTypeKind = $"record {fullTypeKind}";
                TypeInfo typeInfo = new()
                {
                    Namespace = data.ContainingNamespace.ToDisplayString(),
                    ClassName = data.Name,
                    FullTypeKind = fullTypeKind,
                    Visibility = data.DeclaredAccessibility.ToString().ToLower(),
                };

                return new Model()
                {
                    TypeInfo = typeInfo,
                    ContainingType = containingType,
                    Fields = fields,
                };
            }
        ).Where(static m => m is not null);

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var reflectImpl = model.TypeInfo.Attributes.Contains("Pollus.Engine.Reflect.ReflectAttribute") ? null : ReflectGenerator.GetReflectImpl(model);
            var tweenImpl = model.TypeInfo.Attributes.Contains("Pollus.Engine.Tween.TweenAttribute") ? null : TweenGenerator.GetTweenImpl(model);

            List<string> interfaces = [];
            if (reflectImpl != null) interfaces.Add($"Pollus.Engine.Reflect.IReflect<{model.TypeInfo.ClassName}>");
            if (tweenImpl != null) interfaces.Add("Pollus.Engine.Tween.ITweenable");

            var partialExt =
                $$"""
                  {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.ClassName}}
                    : {{string.Join(", ", interfaces)}}
                  {
                      {{reflectImpl}}
                      {{tweenImpl}}
                  } 
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
                  using System.Reflection;

                  {{partialExt}}
                  """, Encoding.UTF8);

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.ClassName ?? "root"}_{model.TypeInfo.ClassName}.Component.gen.cs", source);
        });
    }
}