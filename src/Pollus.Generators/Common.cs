using System.Collections.Immutable;

namespace Pollus.Generators;

using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

class TypeInfo
{
    public string Namespace;
    public string ClassName;
    public string FullTypeKind;
    public string Visibility;

    public bool IsUnmanaged;

    public string[] Attributes;
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

internal static class Common
{
    public static void CollectFields(ITypeSymbol type, List<Field> fields)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is IFieldSymbol { IsStatic: false, IsConst: false, IsAbstract: false, IsImplicitlyDeclared: false } field)
            {
                fields.Add(new Field() { Name = field.Name, Type = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) });
            }
            else if (member is IPropertySymbol { IsStatic: false, IsAbstract: false, IsImplicitlyDeclared: false } property)
            {
                fields.Add(new Field() { Name = property.Name, Type = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) });
            }
        }
    }

    public static Model CollectType(GeneratorSyntaxContext context, CancellationToken token)
    {
        var data = context.SemanticModel.GetDeclaredSymbol(context.Node, token) as ITypeSymbol;
        return CollectType(data);
    }

    public static Model CollectType(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        return CollectType(context.TargetSymbol as ITypeSymbol);
    }

    public static Model CollectType(ITypeSymbol data)
    {
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
            IsUnmanaged = data.IsUnmanagedType,
        };

        return new Model()
        {
            TypeInfo = typeInfo,
            ContainingType = containingType,
            Fields = fields,
        };
    }
}