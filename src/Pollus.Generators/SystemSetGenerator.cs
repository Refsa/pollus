namespace Pollus.Generators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [Generator]
    public class SystemSetGenerator : IIncrementalGenerator
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
            public List<SystemModel> SystemModels;
        }

        class SystemModel
        {
            public Field DescriptorField;
            public Method SystemCallbackMethod;
        }

        class Field
        {
            public string Name;
            public string Type;
        }

        class Method
        {
            public string Name;
            public string ReturnType;
            public List<string> Parameters;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Pollus.ECS.SystemSetAttribute",
                predicate: static (syntaxNode, cancellationToken) =>
                    (syntaxNode is ClassDeclarationSyntax classDecl && classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                    || (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword) && recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword)),
                transform: static (context, cancellationToken) =>
                {
                    var attribute = context.Attributes.FirstOrDefault(e => e.AttributeClass.Name == "SystemSetAttribute");
                    var data = context.TargetSymbol as ITypeSymbol;

                    var systemModels = new List<SystemModel>();
                    foreach (var field in data.GetMembers())
                    {
                        if (field is not IFieldSymbol fieldSymbol) continue;

                        var systemAttr = field.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "SystemAttribute");
                        if (systemAttr == null) continue;

                        var systemCallbackMethod = data.GetMembers().FirstOrDefault(e => e.Name == systemAttr.ConstructorArguments[0].Value as string);
                        if (systemCallbackMethod == null) continue;

                        if (systemCallbackMethod is not IMethodSymbol methodSymbol) continue;

                        systemModels.Add(new SystemModel()
                        {
                            DescriptorField = new()
                            {
                                Name = fieldSymbol.Name,
                                Type = fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            },
                            SystemCallbackMethod = new()
                            {
                                Name = methodSymbol.Name,
                                ReturnType = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                                Parameters = methodSymbol.Parameters.Select(e => e.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).ToList(),
                            },
                        });
                    }

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
                        SystemModels = systemModels,
                    };
                }
            );

            context.RegisterSourceOutput(pipeline, (context, model) =>
            {
                var setups = new StringBuilder();
                foreach (var systemModel in model.SystemModels)
                {
                    var lowerName = systemModel.DescriptorField.Name.ToLower();

                    setups.Append($$"""
        var {{lowerName}} = {{systemModel.DescriptorField.Name}};
        if (string.IsNullOrEmpty({{lowerName}}.Label.Value))
        {
            {{lowerName}}.Label = new SystemLabel("{{model.TypeInfo.ClassName}}::{{systemModel.DescriptorField.Name.Replace("Descriptor", "")}}");
        }
        schedule.AddSystems({{lowerName}}.Stage, FnSystem.Create({{lowerName}}, (SystemDelegate<{{string.Join(", ", systemModel.SystemCallbackMethod.Parameters)}}>){{systemModel.SystemCallbackMethod.Name}}));


""");
                }

                var classTemplate = $$"""
public partial class {{model.TypeInfo.ClassName}} : ISystemSet
{
    public static void AddToSchedule(Schedule schedule)
    {
{{setups}}
    }
}
""";

                if (model.ContainingType != null)
                {
                    classTemplate = $$"""
                    {{model.ContainingType.Visibility}} partial {{model.ContainingType.FullTypeKind}} {{model.ContainingType.ClassName}}
                    {
                        {{classTemplate}}
                    }
                    """;
                }

                var source = SourceText.From($$"""
                namespace {{model.TypeInfo.Namespace}};
                using Pollus.ECS;
                {{classTemplate}}
                """, Encoding.UTF8);

                context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.TypeInfo.ClassName}.SystemSet.gen.cs", source);
            });
        }
    }
}