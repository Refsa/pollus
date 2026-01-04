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
    public string FullyQualifiedClassName;
    public string FileName;
    public string FullTypeKind;
    public string Visibility;

    public bool IsUnmanaged;
    public bool IsGeneric => GenericArguments is not null && GenericArguments.Count > 0;

    public Attribute[] Attributes;
    public string[] Interfaces;

    public List<GenericArgument> GenericArguments;
}

class GenericArgument
{
    public TypeInfo TypeInfo;
    public List<string> Constraints = new();
}

class Attribute
{
    public string Name;
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
    public string FullClassName;
    public bool IsRequired;
    public string[] Attributes;
    public bool IsBlittable;
    public string[] TypeAttributes;
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
                data = new Field()
                {
                    Name = field.Name,
                    FullClassName = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Type = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    IsRequired = field.IsRequired,
                    IsBlittable = field.Type.IsUnmanagedType,
                    TypeAttributes = field.Type.GetAttributes().Select(a => a.ToString()).ToArray(),
                };
            }
            else if (member is IPropertySymbol { IsStatic: false, IsAbstract: false, IsImplicitlyDeclared: false, IsReadOnly: false } property)
            {
                data = new Field()
                {
                    Name = property.Name,
                    FullClassName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Type = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    IsRequired = property.IsRequired,
                    IsBlittable = property.Type.IsUnmanagedType,
                    TypeAttributes = property.Type.GetAttributes().Select(a => a.ToString()).ToArray(),
                };
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
            FullyQualifiedClassName = data.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            FullTypeKind = fullTypeKind,
            Visibility = data.DeclaredAccessibility.ToString().ToLower(),
            IsUnmanaged = data.IsUnmanagedType,
            Interfaces = data.Interfaces.Select(e => e.Name).ToArray(),
            Attributes = data.GetAttributes().Select(a => new Attribute()
            {
                Name = a.ToString(),
                GenericArguments = a.AttributeClass?.TypeArguments.Select(CreateTypeInfo).ToArray(),
            }).ToArray(),
        };

        if (data is INamedTypeSymbol { IsGenericType: true } namedType)
        {
            typeInfo.GenericArguments = new List<GenericArgument>();
            for (int i = 0; i < namedType.TypeParameters.Length; i++)
            {
                var param = namedType.TypeParameters[i];
                var arg = namedType.TypeArguments[i];
                var constraints = new List<string>();

                if (param.HasUnmanagedTypeConstraint) constraints.Add("unmanaged");
                else if (param.HasValueTypeConstraint) constraints.Add("struct");
                else if (param.HasReferenceTypeConstraint) constraints.Add("class");
                else if (param.HasNotNullConstraint) constraints.Add("notnull");

                foreach (var constraintType in param.ConstraintTypes)
                {
                    constraints.Add(constraintType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }

                if (param.HasConstructorConstraint) constraints.Add("new()");

                typeInfo.GenericArguments.Add(new GenericArgument
                {
                    TypeInfo = CreateTypeInfo(arg),
                    Constraints = constraints
                });
            }

            typeInfo.FullClassName = $"{typeInfo.ClassName}<{string.Join(", ", typeInfo.GenericArguments.Select(e => e.TypeInfo.ClassName))}>";
        }

        typeInfo.FileName = typeInfo.FullClassName.Replace('<', '_').Replace(',', '_').Replace(">", "").Replace("global::", "");

        return typeInfo;
    }
}
