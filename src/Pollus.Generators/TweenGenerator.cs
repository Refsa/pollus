namespace Pollus.Generators;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class TweenGenerator : IIncrementalGenerator
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
        public List<Field> Fields;
    }

    class Field
    {
        public string Name;
        public string Type;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Pollus.Engine.Tween.TweenableAttribute",
            predicate: static (syntaxNode, cancellationToken) =>
                (syntaxNode is StructDeclarationSyntax structDecl && structDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                || (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword) && recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)),
            transform: static (context, cancellationToken) =>
            {
                var attribute = context.Attributes.FirstOrDefault(e => e.AttributeClass.Name == "ShaderTypeAttribute");
                var data = context.TargetSymbol as ITypeSymbol;

                var fields = new List<Field>();
                CollectFields(data, fields);

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
                    Fields = fields,
                };
            }
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var partialExt = $$"""
            {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.ClassName}} : Pollus.Engine.Tween.ITweenable<{{model.TypeInfo.ClassName}}>
            {
                public enum TweenField : byte
                {
            {{string.Join("\n", model.Fields.Select(e => $"        {e.Name},"))}}
                }

                public void SetValue<T>(byte field, T value) => SetValue((TweenField)field, value);
                public void SetValue<T>(TweenField field, T value)
                {
                    switch (field)
                    {
            {{string.Join("\n", model.Fields.Select(e => $"            case TweenField.{e.Name}: {e.Name} = Unsafe.As<T, {e.Type}>(ref value); break;"))}}
                        default: throw new ArgumentException($"Invalid property: {field}", nameof(field));
                    }
                }

                public static void PrepareSystems(Schedule schedule)
                {
            {{string.Join("\n", model.Fields.Select(e => $"        schedule.AddSystems(CoreStage.Update, new TweenSystem<{model.TypeInfo.ClassName}, {e.Type}>());"))}}
                }

                public static byte GetFieldIndex<TField>(Expression<Func<{{model.TypeInfo.ClassName}}, TField>> property)
                {
                    string? fieldName = null;
                    if (property.Body is MemberExpression expr)
                    {
                        fieldName = (expr.Member as FieldInfo)?.Name;
                    }

                    if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException("Invalid property expression", nameof(property));
                    return (byte)Enum.Parse<TweenField>(fieldName);
                }
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
            using System.Runtime.CompilerServices;
            using System.Linq.Expressions;
            using System.Reflection;
            using Pollus.Engine.Tween;
            using Pollus.ECS;

            {{partialExt}}
            """, Encoding.UTF8);

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.ClassName ?? "root"}_{model.TypeInfo.ClassName}.Tweenable.gen.cs", source);
        });
    }

    static void CollectFields(ITypeSymbol type, List<Field> fields)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is IFieldSymbol field && !field.IsStatic && !field.IsConst && !field.IsAbstract)
            {
                fields.Add(new Field() { Name = field.Name, Type = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) });
            }
        }
    }
}