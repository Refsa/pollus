namespace Pollus.Generators;

using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class ReflectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Pollus.Utils.ReflectAttribute",
            predicate: static (syntaxNode, cancellationToken) =>
                (syntaxNode is StructDeclarationSyntax structDecl && structDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                || (syntaxNode is RecordDeclarationSyntax recordDecl && recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword) && recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)),
            transform: Common.CollectType
        );

        context.RegisterSourceOutput(pipeline, (context, model) =>
        {
            var partialExt =
                $$"""
                  {{model.TypeInfo.Visibility}} partial {{model.TypeInfo.FullTypeKind}} {{model.TypeInfo.FullClassName}} : Pollus.Utils.IReflect<{{model.TypeInfo.FullClassName}}>
                  {
                      {{GetReflectImpl(model)}}
                  } 
                  """;

            if (model.ContainingType != null)
            {
                partialExt =
                    $$"""
                      {{model.ContainingType.Visibility}} partial {{model.ContainingType.FullTypeKind}} {{model.ContainingType.FullClassName}}
                      {
                          {{partialExt}}
                      }
                      """;
            }

            var source = SourceText.From(
                $$"""
                  namespace {{model.TypeInfo.Namespace}};
                  using System.Runtime.CompilerServices;
                  using System.Linq.Expressions;
                  using System.Reflection;

                  {{partialExt}}
                  """, Encoding.UTF8);

            context.AddSource($"{model.TypeInfo.Namespace.Replace('.', '_')}_{model.ContainingType?.FileName ?? "root"}_{model.TypeInfo.FileName}.Reflect.gen.cs", source);
        });
    }

    internal static string GetReflectImpl(Model model)
    {
        return
            $$"""
                  public enum ReflectField : byte
                  {
              {{string.Join("\n", model.Fields.Select(e => $"        {e.Name},"))}}
                  }

                  static readonly byte[] reflectFields = new byte[] {
              {{string.Join(", ", model.Fields.Select((_, i) => i))}}
                  };
                  public static byte[] Fields => reflectFields;

                  [MethodImpl(MethodImplOptions.AggressiveInlining)]
                  public void SetValue<TField>(byte field, TField value) => SetValue((ReflectField)field, value);

                  [MethodImpl(MethodImplOptions.AggressiveInlining)]
                  public void SetValue<TField>(ReflectField field, TField value)
                  {
                      switch (field)
                      {
              {{string.Join("\n", model.Fields.Select(e => $"            case ReflectField.{e.Name}: {e.Name} = Unsafe.As<TField, {e.Type}>(ref value); break;"))}}
                          default: throw new ArgumentException($"Invalid property: {field}", nameof(field));
                      }
                  }

                  [MethodImpl(MethodImplOptions.AggressiveInlining)]
                  public static byte GetFieldIndex<TField>(Expression<Func<{{model.TypeInfo.FullClassName}}, TField>> property)
                  {
                      string? fieldName = null;
                      if (property.Body is MemberExpression expr)
                      {
                          fieldName = (expr.Member as FieldInfo)?.Name;
                      }

                      if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException("Invalid property expression", nameof(property));

                      return GetFieldIndex(fieldName);
                  }

                  [MethodImpl(MethodImplOptions.AggressiveInlining)]
                  public static byte GetFieldIndex(string fieldName)
                  {
                      if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException("Invalid property expression", fieldName);
                      return (byte)Enum.Parse<ReflectField>(fieldName);
                  }
              """;
    }
}