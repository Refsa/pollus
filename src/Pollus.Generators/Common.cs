namespace Pollus.Generators;

using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

class TypeInfo
{
    public string Namespace;
    public string ClassName;
    public string FullClassName;
    public string FileName;
    public string FullTypeKind;
    public string Visibility;

    public bool IsUnmanaged;
    public bool IsGeneric => GenericArguments is not null && GenericArguments.Length > 0;

    public string[] Attributes;

    public TypeInfo[] GenericArguments;
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
    public string[] Attributes;
}

internal static class Common
{
    public static void CollectFields(ITypeSymbol type, List<Field> fields)
    {
        foreach (var member in type.GetMembers())
        {
            Field data = null;

            if (member is IFieldSymbol { IsStatic: false, IsConst: false, IsAbstract: false, IsImplicitlyDeclared: false, IsReadOnly: false } field)
            {
                data = new Field() { Name = field.Name, Type = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) };
            }
            else if (member is IPropertySymbol { IsStatic: false, IsAbstract: false, IsImplicitlyDeclared: false, IsReadOnly: false } property)
            {
                data = new Field() { Name = property.Name, Type = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) };
            }

            if (data is null) continue;

            data.Attributes = member.GetAttributes().Select(a => a.ToString()).ToArray();
            fields.Add(data);
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
        CollectFields(data, fields);

        TypeInfo containingType = null;
        if (data.ContainingType?.TypeKind is TypeKind.Struct or TypeKind.Class or TypeKind.Interface)
        {
            var containingTypeKind = data.ContainingType.TypeKind.ToString().ToLower();
            if (data.ContainingType.IsRecord && containingTypeKind != "record") containingTypeKind = $"record {containingTypeKind}";
            containingType = CreateTypeInfo(data.ContainingType);
        }

        var typeInfo = CreateTypeInfo(data);

        return new Model()
        {
            TypeInfo = typeInfo,
            ContainingType = containingType,
            Fields = fields,
        };
    }

    public static TypeInfo CreateTypeInfo(ITypeSymbol data)
    {
        var fullTypeKind = data.TypeKind.ToString().ToLower();
        if (data.IsRecord && fullTypeKind != "record") fullTypeKind = $"record {fullTypeKind}";

        var typeInfo = new TypeInfo()
        {
            Namespace = data.ContainingNamespace?.ToDisplayString(),
            ClassName = data.Name,
            FullClassName = data.Name,
            FullTypeKind = fullTypeKind,
            Visibility = data.DeclaredAccessibility.ToString().ToLower(),
            IsUnmanaged = data.IsUnmanagedType,
            Attributes = data.GetAttributes().Select(a => a.ToString()).ToArray(),
        };

        if (data is INamedTypeSymbol { IsGenericType: true } namedType)
        {
            typeInfo.GenericArguments = namedType.TypeArguments
                .Select(CreateTypeInfo)
                .ToArray();
            typeInfo.FullClassName = $"{typeInfo.ClassName}<{string.Join(", ", typeInfo.GenericArguments.Select(e => e.ClassName))}>";
        }

        typeInfo.FileName = typeInfo.FullClassName.Replace('<', '_').Replace(',', '_').Replace(">", "");

        return typeInfo;
    }
}
