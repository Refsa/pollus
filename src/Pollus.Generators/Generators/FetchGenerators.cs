namespace Pollus.ECS.Generators;

using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class FetchGenerator : IIncrementalGenerator
{
    class Target
    {
        public string Namespace;
        public string ClassName;
    }
    class Model
    {
        public Target Data;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // SetupForAttribute(context, "Pollus.ECS.ResourceAttribute", "Pollus.ECS.ResourceFetch<{0}>.Register();");
        // SetupForAttribute(context, "Pollus.Engine.Assets.AssetAttribute", "Pollus.Engine.Assets.AssetsFetch<{0}>.Register();");
    }

    void SetupForAttribute(IncrementalGeneratorInitializationContext context, string attributeName, string fetchFormat)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: attributeName,
            predicate: static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax or StructDeclarationSyntax,
            transform: static (context, cancellationToken) =>
            {
                var attribute = context.Attributes.FirstOrDefault(e => e.AttributeClass.Name == "FetchAttribute");
                var data = context.TargetSymbol;

                return new Model()
                {
                    Data = new Target()
                    {
                        Namespace = data.ContainingNamespace.ToDisplayString(),
                        ClassName = data.Name
                    }
                };
            }
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var fullName = $"{model.Data.Namespace}.{model.Data.ClassName}";

            var sourceText = SourceText.From($$"""
                namespace Pollus.Generated.Fetch;
                using System.Runtime.CompilerServices;

                internal static class {{model.Data.ClassName}}Init
                {
                    [ModuleInitializer]
                    public static void Init()
                    {
                        {{fetchFormat.Replace("{0}", fullName)}}
                    }
                }
                """, Encoding.UTF8);

            context.AddSource($"FetchInit_{model.Data.Namespace.Replace('.', '_')}_{model.Data.ClassName}.gen.cs", sourceText);
        });
    }
}