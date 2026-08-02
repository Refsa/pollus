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
        const string CoroutineReturnType = "global::System.Collections.Generic.IEnumerable<global::Pollus.Coroutine.Yield>";
        const string DefaultStage = "Update";

        class Model
        {
            public TypeInfo TypeInfo;
            public TypeInfo ContainingType;
            public List<SystemModel> SystemModels;
        }

        class SystemModel
        {
            public Method SystemCallbackMethod;
            public bool IsCoroutine;
            public string Label;
            public string Stage;
            public string RunCriteria;
            public string DescriptorCallback;
            public bool IsExclusive;
            public List<string> RunsAfter = new();
            public List<string> RunsBefore = new();
            public List<string> Dependencies = new();
            public List<string> Locals = new();
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

                    var typeInfo = Common.CreateTypeInfo(data);

                    string systemGroupRunCriteria = null;
                    if (context.Attributes[0] is { ConstructorArguments.Length: > 0 } attr
                        && attr.ConstructorArguments[0] is { Type.TypeKind: TypeKind.Enum } arg)
                    {
                        systemGroupRunCriteria = CreateStateRunCriteria(arg, "OnCurrent");
                    }

                    var systems = new List<(IMethodSymbol Method, AttributeData Attribute)>();
                    foreach (var member in data.GetMembers())
                    {
                        if (member is not IMethodSymbol methodSymbol) continue;

                        var systemAttr = member.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "SystemAttribute");
                        if (systemAttr is null) continue;

                        systems.Add((methodSymbol, systemAttr));
                    }

                    // labels are resolved up front so siblings can be referenced with nameof(Method)
                    var labels = new Dictionary<string, string>();
                    foreach (var (method, attribute) in systems)
                    {
                        labels[method.Name] = GetNamedArgument(attribute, "Label")?.Value as string
                            ?? $"{typeInfo.FullClassName}::{method.Name}";
                    }

                    var systemModels = new List<SystemModel>();
                    foreach (var (method, attribute) in systems)
                    {
                        systemModels.Add(CreateSystemModel(data, method, attribute, labels, systemGroupRunCriteria));
                    }

                    return new Model()
                    {
                        TypeInfo = typeInfo,
                        ContainingType = data.ContainingType != null ? Common.CreateTypeInfo(data.ContainingType) : null,
                        SystemModels = systemModels,
                    };
                }
            );

            context.RegisterSourceOutput(pipeline, (context, model) =>
            {
                var setups = new StringBuilder();
                foreach (var systemModel in model.SystemModels)
                {
                    setups.AppendLine(CreateSetup(systemModel));
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

        static SystemModel CreateSystemModel(ITypeSymbol owner, IMethodSymbol method, AttributeData systemAttribute,
            Dictionary<string, string> labels, string systemGroupRunCriteria)
        {
            var model = new SystemModel()
            {
                Label = labels[method.Name],
                IsCoroutine = FullyQualified(method.ReturnType) == CoroutineReturnType,
                DescriptorCallback = GetNamedArgument(systemAttribute, "DescriptorCallback")?.Value as string,
                IsExclusive = GetNamedArgument(systemAttribute, "IsExclusive")?.Value is true,
                SystemCallbackMethod = new()
                {
                    Name = method.Name,
                    ReturnType = FullyQualified(method.ReturnType),
                    Parameters = method.Parameters.Select(e => FullyQualified(e.Type)).ToList(),
                },
            };

            if (systemAttribute.ConstructorArguments.Length > 0
                && systemAttribute.ConstructorArguments[0].Value is string stage)
            {
                model.Stage = Literal(stage);
            }

            foreach (var value in GetArrayArgument(systemAttribute, "RunsAfter"))
            {
                if (value.Value is string label) model.RunsAfter.Add(ResolveLabel(label, labels));
            }

            foreach (var value in GetArrayArgument(systemAttribute, "RunsBefore"))
            {
                if (value.Value is string label) model.RunsBefore.Add(ResolveLabel(label, labels));
            }

            foreach (var value in GetArrayArgument(systemAttribute, "Dependencies"))
            {
                if (value.Value is ITypeSymbol dependency) model.Dependencies.Add($"typeof({FullyQualified(dependency)})");
            }

            var runCriterias = new List<string>();
            if (!string.IsNullOrEmpty(systemGroupRunCriteria)) runCriterias.Add(systemGroupRunCriteria);

            foreach (var attribute in method.GetAttributes())
            {
                switch (attribute.AttributeClass?.Name)
                {
                    case "RunsAfterAttribute":
                        model.RunsAfter.Add($"{FullyQualified(attribute.AttributeClass.TypeArguments[0])}.Label");
                        break;
                    case "RunsBeforeAttribute":
                        model.RunsBefore.Add($"{FullyQualified(attribute.AttributeClass.TypeArguments[0])}.Label");
                        break;
                    case "LocalAttribute":
                        model.Locals.Add(CreateLocal(owner, attribute));
                        break;
                    case "RunCriteriaAttribute":
                        runCriterias.Add(CreateRunCriteria(attribute));
                        break;
                    case "InStateAttribute":
                        runCriterias.Add(CreateStateRunCriteria(attribute.ConstructorArguments[0], "OnCurrent"));
                        break;
                    case "OnStateEnterAttribute":
                        if (model.Stage is null) model.Stage = $"StateEnter.On({attribute.ConstructorArguments[0].ToCSharpString()})";
                        else runCriterias.Add(CreateStateRunCriteria(attribute.ConstructorArguments[0], "OnEnter"));
                        break;
                    case "OnStateExitAttribute":
                        if (model.Stage is null) model.Stage = $"StateExit.On({attribute.ConstructorArguments[0].ToCSharpString()})";
                        else runCriterias.Add(CreateStateRunCriteria(attribute.ConstructorArguments[0], "OnExit"));
                        break;
                }
            }

            model.Stage ??= Literal(DefaultStage);
            model.RunCriteria = runCriterias.Count switch
            {
                0 => null,
                1 => runCriterias[0],
                _ => $"WhenAll.Create({string.Join(", ", runCriterias)})",
            };

            return model;
        }

        static string CreateSetup(SystemModel model)
        {
            var name = model.SystemCallbackMethod.Name;
            var descriptor = $"{char.ToLowerInvariant(name[0])}{name.Substring(1)}Descriptor";
            var parameters = model.SystemCallbackMethod.Parameters;

            var initializers = new List<string>() { $"Stage = {model.Stage}," };
            if (model.RunsAfter.Count > 0) initializers.Add($"RunsAfter = [{string.Join(", ", model.RunsAfter)}],");
            if (model.RunsBefore.Count > 0) initializers.Add($"RunsBefore = [{string.Join(", ", model.RunsBefore)}],");
            if (model.Dependencies.Count > 0) initializers.Add($"Dependencies = [{string.Join(", ", model.Dependencies)}],");
            if (model.Locals.Count > 0) initializers.Add($"Locals = [{string.Join(", ", model.Locals)}],");
            if (model.RunCriteria != null) initializers.Add($"RunCriteria = {model.RunCriteria},");
            if (model.IsExclusive) initializers.Add("IsExclusive = true,");

            string create;
            if (model.IsCoroutine)
            {
                var parameter = parameters.Count == 0 ? "EmptyParam" : $"Param<{string.Join(", ", parameters)}>";
                var arguments = string.Join(", ", parameters.Select((e, i) => $"param.Param{i}"));
                create = $"Coroutine.Create({descriptor}, static ({parameter} param) => {name}({arguments}))";
            }
            else
            {
                var delegateType = parameters.Count == 0 ? "SystemDelegate" : $"SystemDelegate<{string.Join(", ", parameters)}>";
                create = $"FnSystem.Create({descriptor}, ({delegateType}){name})";
            }

            var setup = new StringBuilder();
            setup.AppendLine($"var {descriptor} = new SystemBuilderDescriptor({Literal(model.Label)})");
            setup.AppendLine("{");
            foreach (var initializer in initializers) setup.AppendLine($"    {initializer}");
            setup.AppendLine("};");
            if (!string.IsNullOrEmpty(model.DescriptorCallback)) setup.AppendLine($"{model.DescriptorCallback}(ref {descriptor});");
            setup.AppendLine($"schedule.AddSystems({descriptor}.Stage, {create});");

            return Indent(setup.ToString(), "        ");
        }

        static string CreateLocal(ITypeSymbol owner, AttributeData attribute)
        {
            var type = FullyQualified(attribute.AttributeClass.TypeArguments[0]);

            if (attribute.ConstructorArguments.Length == 0) return $"Local.From<{type}>(new {type}())";
            if (IsFactoryArgument(attribute)) return $"Local.From<{type}>({MemberAccess(owner, "", (string)attribute.ConstructorArguments[0].Value)})";
            return $"Local.From<{type}>({attribute.ConstructorArguments[0].ToCSharpString()})";
        }

        static string CreateRunCriteria(AttributeData attribute)
        {
            var typeSymbol = attribute.AttributeClass.TypeArguments[0];
            var type = FullyQualified(typeSymbol);

            if (attribute.ConstructorArguments.Length == 0) return $"new {type}()";
            if (IsFactoryArgument(attribute)) return MemberAccess(typeSymbol, $"{type}.", (string)attribute.ConstructorArguments[0].Value);
            return $"new {type}({attribute.ConstructorArguments[0].ToCSharpString()})";
        }

        static string CreateStateRunCriteria(TypedConstant state, string transition)
        {
            return $"StateRunCriteria<{FullyQualified(state.Type)}>.{transition}({state.ToCSharpString()})";
        }

        static string ResolveLabel(string label, Dictionary<string, string> labels)
        {
            return Literal(labels.TryGetValue(label, out var resolved) ? resolved : label);
        }

        static string MemberAccess(ITypeSymbol owner, string prefix, string memberName)
        {
            var member = owner.GetMembers(memberName).FirstOrDefault();
            return member is IMethodSymbol ? $"{prefix}{memberName}()" : $"{prefix}{memberName}";
        }

        static bool IsFactoryArgument(AttributeData attribute)
        {
            return attribute.AttributeConstructor is { Parameters.Length: 1 } constructor && constructor.Parameters[0].Name == "factory";
        }

        static TypedConstant? GetNamedArgument(AttributeData attribute, string name)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == name) return argument.Value;
            }
            return null;
        }

        static IEnumerable<TypedConstant> GetArrayArgument(AttributeData attribute, string name)
        {
            if (GetNamedArgument(attribute, name) is { Kind: TypedConstantKind.Array, IsNull: false } argument) return argument.Values;
            return Enumerable.Empty<TypedConstant>();
        }

        static string FullyQualified(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);

        static string Indent(string value, string indent)
        {
            var lines = value.TrimEnd().Split('\n');
            return string.Join("\n", lines.Select(e => $"{indent}{e.TrimEnd('\r')}"));
        }
    }
}
