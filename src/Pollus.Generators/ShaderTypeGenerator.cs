namespace Pollus.Generators;

using System;
using System.Diagnostics;
using System.Linq;
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
        public string ContainingType;
        public string ClassName;
        public string FullTypeKind;
        public string Visibility;
        public bool IsPartial;
    }

    class Model
    {
        public TypeInfo ContainingType;
        public string Namespace;
        public string ClassName;
        public bool IsPartial;
        public bool IsRecordStruct;
        public string Visibility;
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
                if (data.ContainingType?.TypeKind is TypeKind.Struct or TypeKind.Class)
                {
                    containingType = new TypeInfo()
                    {
                        Namespace = data.ContainingNamespace.ToDisplayString(),
                        ClassName = data.ContainingType.Name,
                        FullTypeKind = data.ContainingType.TypeKind.ToString().ToLower(),
                        IsPartial = data.ContainingType.DeclaringSyntaxReferences.Any(e => e.GetSyntax().IsKind(SyntaxKind.PartialKeyword)),
                        Visibility = data.ContainingType.DeclaredAccessibility.ToString().ToLower(),
                    };
                }

                return new Model()
                {
                    Namespace = data.ContainingNamespace.ToDisplayString(),
                    ClassName = data.Name,
                    IsPartial = data.DeclaringSyntaxReferences.Any(e => e.GetSyntax().IsKind(SyntaxKind.PartialKeyword)),
                    IsRecordStruct = data.IsRecord,
                    Visibility = data.DeclaredAccessibility.ToString().ToLower(),
                    SizeOf = sizeOf,
                    AlignOf = alignOf,
                    ContainingType = containingType,
                };
            }
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var partialExt = $$"""
            {{model.Visibility}} partial {{(model.IsRecordStruct ? "record" : "")}} struct {{model.ClassName}} : Pollus.Graphics.IShaderType
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
            namespace {{model.Namespace}};
            {{partialExt}}
            """, Encoding.UTF8);

            context.AddSource($"{model.Namespace.Replace('.', '_')}_{model.ContainingType?.ClassName ?? "root"}_{model.ClassName}.ShaderType.gen.cs", source);
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
                if (member is IFieldSymbol field)
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
            _ => false,
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
            _ => throw new NotSupportedException(),
        };
    }
}