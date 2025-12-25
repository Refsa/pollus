namespace Pollus.Generators;

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class ShaderTypeGenerator : IIncrementalGenerator
{
    class TypeInfo
    {
        public string Namespace;
        public string ClassName;
        public string FullTypeKind;
        public string Visibility;
    }

    class Model
    {
        public TypeInfo TypeInfo;
        public TypeInfo ContainingType;
        public int SizeOf;
        public int AlignOf;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Pollus.Graphics.ShaderTypeAttribute",
            predicate: static (syntaxNode, cancellationToken) =>
                (syntaxNode is StructDeclarationSyntax structDecl && structDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                || (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword) && recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)),
            transform: static (context, cancellationToken) =>
            {
                var attribute = context.Attributes.FirstOrDefault(e => e.AttributeClass.Name == "ShaderTypeAttribute");
                var data = context.TargetSymbol as ITypeSymbol;

                var alignOf = 0;
                var sizeOf = CollectPrimitiveTypes(data, ref alignOf);

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
                    SizeOf = sizeOf,
                    AlignOf = alignOf,
                };
            }
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var partialExt = $$"""
            {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.ClassName}} : Pollus.Graphics.IShaderType
            {
                public static uint SizeOf => {{model.SizeOf}};
                public static uint AlignOf => {{model.AlignOf}};
            } 
            """;

            if (model.ContainingType != null)
            {
                partialExt = $$"""
                {{model.ContainingType.Visibility}} partial {{model.ContainingType.FullTypeKind}} {{model.ContainingType.ClassName}}
                {
                    {{partialExt}}
                }
                """;
            }

            var source = SourceText.From($$"""
            namespace {{model.TypeInfo.Namespace}};
            {{partialExt}}
            """, Encoding.UTF8);

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.ClassName ?? "root"}_{model.TypeInfo.ClassName}.ShaderType.gen.cs", source);
        });
    }

    static int CollectPrimitiveTypes(ITypeSymbol type, ref int maxAlignOf)
    {
        if (IsPrimitive(type))
        {
            return GetPrimitiveSize(type);
        }
        else if (type.TypeKind == TypeKind.Struct)
        {
            var totalSize = 0;
            foreach (var member in type.GetMembers())
            {
                if (member is IFieldSymbol field && !field.IsStatic && !field.IsConst && !field.IsAbstract)
                {
                    totalSize += CollectPrimitiveTypes(field.Type, ref maxAlignOf);
                }
            }

            var alignOf = totalSize switch
            {
                <= 2 => 2,
                <= 4 => 4,
                <= 8 => 8,
                > 8 => 16,
            };

            maxAlignOf = Math.Max(maxAlignOf, alignOf);
            return totalSize;
        }

        return 0;
    }

    static bool IsPrimitive(ITypeSymbol type)
    {
        return type.Name switch
        {
            "Bool" => true,
            "Byte" => true,
            "SByte" => true,
            "Int16" => true,
            "UInt16" => true,
            "Int32" => true,
            "UInt32" => true,
            "Int64" => true,
            "UInt64" => true,
            "Single" => true,
            "Double" => true,
            _ => type.TypeKind switch
            {
                TypeKind.Enum => true,
                _ => false,
            },
        };
    }

    static int GetPrimitiveSize(ITypeSymbol type)
    {
        return type.Name switch
        {
            "Bool" => 1,
            "Byte" => 1,
            "SByte" => 1,
            "Int16" => 2,
            "UInt16" => 2,
            "Int32" => 4,
            "UInt32" => 4,
            "Int64" => 8,
            "UInt64" => 8,
            "Single" => 4,
            "Double" => 8,
            _ => type.TypeKind switch
            {
                TypeKind.Enum => GetPrimitiveSize((type as INamedTypeSymbol)?.EnumUnderlyingType),
                _ => throw new NotSupportedException(),
            },
        };
    }
}