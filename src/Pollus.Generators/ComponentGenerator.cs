namespace Pollus.Generators;

using System.CodeDom.Compiler;
using System.IO;
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
                    return structDecl.Modifiers.Any(SyntaxKind.PartialKeyword)
                           && structDecl.BaseList is { Types.Count: > 0 }
                           && HasComponentBase(structDecl.BaseList);
                }

                if (syntaxNode is RecordDeclarationSyntax recordDecl)
                {
                    return recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                           && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword)
                           && recordDecl.BaseList is { Types.Count: > 0 }
                           && HasComponentBase(recordDecl.BaseList);
                }

                return false;
            },
            transform: static (context, token) =>
            {
                var typeSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node, token) as INamedTypeSymbol;
                if (typeSymbol is null) return null;

                if (!typeSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "Pollus.ECS.IComponent"))
                    return null;

                return Common.CollectType(typeSymbol);
            }
        ).Where(static m => m is not null);

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            if (model is null) return;

            const string worldSerializationContextType = "WorldSerializationContext";

            var reflectImpl = model.TypeInfo.Attributes.Contains("Pollus.Utils.ReflectAttribute") ? null : ReflectGenerator.GetReflectImpl(model);
            var tweenImpl = model.TypeInfo.Attributes.Contains("Pollus.Engine.Tween.TweenAttribute") ? null : TweenGenerator.GetTweenImpl(model);

            var serializeImpl = model.TypeInfo.Attributes.Contains("Pollus.Core.Serialization.SerializeAttribute")
                ? null
                : $$"""
                    {{SerializeGenerator.GetISerializableImpl(model, worldSerializationContextType)}}
                    {{(model.ContainingType is null ? SerializeGenerator.GetModuleInitializerImpl(model, worldSerializationContextType) : "")}}
                    """;
            var serializerImpl = string.IsNullOrEmpty(serializeImpl) ? null : SerializeGenerator.GetSerializerImpl(model, worldSerializationContextType);

            List<string> interfaces = [];
            if (!string.IsNullOrEmpty(reflectImpl)) interfaces.Add($"Pollus.Utils.IReflect<{model.TypeInfo.FullClassName}>");
            if (!string.IsNullOrEmpty(tweenImpl)) interfaces.Add("Pollus.Engine.Tween.ITweenable");
            if (!string.IsNullOrEmpty(serializeImpl)) interfaces.Add($"Pollus.Core.Serialization.ISerializable<{worldSerializationContextType}>");

            var partialExt =
                $$"""
                  {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.FullClassName}}
                    : {{string.Join(", ", interfaces)}}
                  {
                      {{reflectImpl}}
                      {{tweenImpl}}

                      {{serializeImpl}}
                  }

                  {{serializerImpl}}
                  """;

            if (model.ContainingType != null)
            {
                partialExt =
                    $$"""
                      {{model.ContainingType.Visibility}} partial {{model.ContainingType.FullTypeKind}} {{model.ContainingType.FullClassName}}
                      {
                          {{partialExt}}

                          {{(!string.IsNullOrEmpty(serializeImpl) ? SerializeGenerator.GetModuleInitializerImpl(model, worldSerializationContextType) : "")}}
                      }
                      """;
            }

            var indentedTextWriter = new IndentedTextWriter(new StringWriter(), "  ");
            indentedTextWriter.Write(
                $$"""
                  namespace {{model.TypeInfo.Namespace}};
                  using System.Runtime.CompilerServices;
                  using System.Linq.Expressions;
                  using System.Reflection;
                  using Pollus.ECS;
                  using Pollus.Engine.Tween;
                  using Pollus.Utils;
                  using Pollus.Core.Serialization;
                  using Pollus.Engine.Serialization;

                  {{partialExt}}
                  """);

            var source = SourceText.From(indentedTextWriter.InnerWriter.ToString(), Encoding.UTF8);

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.FileName ?? "root"}_{model.TypeInfo.FileName}.Component.gen.cs", source);
        });
    }

    static bool HasComponentBase(BaseListSyntax baseList)
    {
        foreach (var baseType in baseList.Types)
        {
            if (baseType.Type.ToString().EndsWith("IComponent"))
                return true;
        }

        return false;
    }
}
