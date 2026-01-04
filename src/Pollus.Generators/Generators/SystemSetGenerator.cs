namespace Pollus.Generators
{
    using System;
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
        class Model
        {
            public TypeInfo TypeInfo;
            public TypeInfo ContainingType;
            public List<SystemModel> SystemModels;
            public List<CoroutineModel> CoroutineModels;
        }

        class SystemModel
        {
            public Field DescriptorField;
            public Method SystemCallbackMethod;
        }

        class CoroutineModel
        {
            public Field DescriptorField;
            public Method CoroutineCallbackMethod;
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
                    var data = context.TargetSymbol as ITypeSymbol;
                    if (data is null) throw new Exception("Target symbol is null");

                    var systemModels = new List<SystemModel>();
                    var coroutineModels = new List<CoroutineModel>();
                    foreach (var field in data.GetMembers())
                    {
                        if (field is not IFieldSymbol fieldSymbol) continue;

                        var systemAttr = field.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "SystemAttribute");
                        if (systemAttr is not null)
                        {
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

                        var coroutineAttr = field.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "CoroutineAttribute");
                        if (coroutineAttr is not null)
                        {
                            var coroutineCallbackMethod = data.GetMembers().FirstOrDefault(e => e.Name == coroutineAttr.ConstructorArguments[0].Value as string);
                            if (coroutineCallbackMethod == null) continue;
                            if (coroutineCallbackMethod is not IMethodSymbol methodSymbol) continue;

                            var returnType = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            if (returnType != "global::System.Collections.Generic.IEnumerable<global::Pollus.Coroutine.Yield>") continue;

                            coroutineModels.Add(new CoroutineModel()
                            {
                                DescriptorField = new()
                                {
                                    Name = fieldSymbol.Name,
                                    Type = fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                                },
                                CoroutineCallbackMethod = new()
                                {
                                    Name = methodSymbol.Name,
                                    ReturnType = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                                    Parameters = methodSymbol.Parameters.Select(e => e.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).ToList(),
                                },
                            });
                        }
                    }

                    return new Model()
                    {
                        TypeInfo = Common.CreateTypeInfo(data),
                        ContainingType = data.ContainingType != null ? Common.CreateTypeInfo(data.ContainingType) : null,
                        SystemModels = systemModels,
                        CoroutineModels = coroutineModels,
                    };
                }
            );

            context.RegisterSourceOutput(pipeline, (context, model) =>
            {
                var setups = new StringBuilder();
                foreach (var systemModel in model.SystemModels)
                {
                    var lowerName = systemModel.DescriptorField.Name.ToLower();

                    setups.Append(
                        $$"""
                                  var {{lowerName}} = {{systemModel.DescriptorField.Name}};
                                  if (string.IsNullOrEmpty({{lowerName}}.Label.Value))
                                  {
                                      {{lowerName}}.Label = new SystemLabel("{{model.TypeInfo.FullClassName}}::{{systemModel.DescriptorField.Name.Replace("Descriptor", "")}}");
                                  }
                                  schedule.AddSystems({{lowerName}}.Stage, FnSystem.Create({{lowerName}}, (SystemDelegate<{{string.Join(", ", systemModel.SystemCallbackMethod.Parameters)}}>){{systemModel.SystemCallbackMethod.Name}}));
                          """);
                }

                foreach (var coroutineModel in model.CoroutineModels)
                {
                    var lowerName = coroutineModel.DescriptorField.Name.ToLower();
                    var paramUnwrap = string.Join(", ", coroutineModel.CoroutineCallbackMethod.Parameters.Select((e, i) => $"param.Param{i}"));

                    setups.Append(
                        $$"""
                                var {{lowerName}} = {{coroutineModel.DescriptorField.Name}};
                                if (string.IsNullOrEmpty({{lowerName}}.Label.Value))
                                {
                                    {{lowerName}}.Label = new SystemLabel("{{model.TypeInfo.FullClassName}}::{{coroutineModel.DescriptorField.Name.Replace("Descriptor", "")}}");
                                }
                                schedule.AddSystems({{lowerName}}.Stage, Coroutine.Create({{lowerName}}, 
                                static (Param<{{string.Join(", ", coroutineModel.CoroutineCallbackMethod.Parameters)}}> param) =>
                                {
                                    return {{coroutineModel.CoroutineCallbackMethod.Name}}({{paramUnwrap}});
                                }
                                ));
                          """);
                }

                var classTemplate =
                    $$"""
                      public partial class {{model.TypeInfo.FullClassName}} : ISystemSet
                      {
                          public static void AddToSchedule(Schedule schedule)
                          {
                      {{setups}}
                          }
                      }
                      """;

                if (model.ContainingType != null)
                {
                    classTemplate =
                        $$"""
                          {{model.ContainingType.Visibility}} partial {{model.ContainingType.FullTypeKind}} {{model.ContainingType.ClassName}}
                          {
                              {{classTemplate}}
                          }
                          """;
                }

                var source = SourceText.From(
                    $$"""
                      namespace {{model.TypeInfo.Namespace}};
                      using Pollus.ECS;
                      {{classTemplate}}
                      """, Encoding.UTF8);

                context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.TypeInfo.ClassName}.SystemSet.gen.cs", source);
            });
        }
    }
}