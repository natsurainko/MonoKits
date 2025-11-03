using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonoKits.SourceGenerators.Content;

[Generator]
public class ContentObjectSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.AttributeLists.Count > 0;
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeSymbol)
                {
                    var attributeContainingType = attributeSymbol.ContainingType;
                    var fullName = attributeContainingType.ToDisplayString();

                    if (fullName.Contains("ContentObjectAttribute") ||
                        fullName.Contains("Texture2DAttribute"))
                    {
                        return classDeclaration;
                    }
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (var classDeclaration in classes)
        {
            if (classDeclaration.SyntaxTree == null)
                continue;

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            if (semanticModel == null)
                continue;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            if (classSymbol == null)
                continue;

            try
            {
                var sourceCode = GenerateClassSource(compilation, classSymbol, classDeclaration);
                if (sourceCode != null)
                {
                    context.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "COG001",
                        "Code generation error",
                        $"Error generating code for {classSymbol.Name}: {ex.Message}",
                        "CodeGeneration",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }
    }

    private static string? GenerateClassSource(Compilation compilation, INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclaration)
    {
        var attributes = GetContentObjectAttributes(classSymbol);
        if (!attributes.Any())
            return null;

        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;

        var propertyDeclarations = new StringBuilder();
        var loadStatements = new StringBuilder();
        var disposeStatements = new StringBuilder();

        bool hasTexture2DAttributes = false;

        foreach (var attr in attributes)
        {
            var propertyName = GetPropertyName(attr);
            var contentPath = GetContentPath(attr);
            var contentType = GetContentType(compilation, attr);

            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(contentPath) || contentType == null)
                continue;

            propertyDeclarations.AppendLine($"    public {GetFullTypeName(contentType)} {propertyName} {{ get; private set; }}");
            propertyDeclarations.AppendLine();

            if (IsTexture2DAttribute(attr))
            {
                hasTexture2DAttributes = true;
                loadStatements.AppendLine($"        this.{propertyName} = global::Microsoft.Xna.Framework.Graphics.Texture2D.FromFile(_graphicsDevice, \"{contentPath}\");");
                disposeStatements.AppendLine($"        {propertyName}?.Dispose();");
            }
            else
            {
                loadStatements.AppendLine($"        this.{propertyName} = _contentManager.Load<{GetFullTypeName(contentType)}>(\"{contentPath}\");");
                disposeStatements.AppendLine($"        _contentManager.UnloadAsset(\"{contentPath}\");");
            }
        }

        var graphicsDeviceField = hasTexture2DAttributes ?
            "    private readonly global::Microsoft.Xna.Framework.Graphics.GraphicsDevice _graphicsDevice = global::MonoGame.Extended.Content.ContentManagerExtensions.GetGraphicsDevice(contentManager);\n\n" :
            "";

        var sourceCode = $$"""
            // <auto-generated/>
            #nullable enable
            
            using Microsoft.Xna.Framework.Content;
            using Microsoft.Xna.Framework.Graphics;
            
            namespace {{namespaceName}};
            
            partial class {{className}}(global::Microsoft.Xna.Framework.Content.ContentManager contentManager) : global::MonoKits.Content.ScopedContent(contentManager)
            {
            {{graphicsDeviceField}}{{propertyDeclarations}}
                public override void Load()
                {
            {{loadStatements}}
                    OnLoaded();
                }
            
                public override void Dispose()
                {
            {{disposeStatements}}
                }
            }
            """;

        return sourceCode;
    }

    private static List<AttributeData> GetContentObjectAttributes(INamedTypeSymbol classSymbol)
    {
        var attributes = new List<AttributeData>();

        foreach (var attribute in classSymbol.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass == null)
                continue;

            var fullName = attributeClass.ToDisplayString();

            if (fullName.Contains("ContentObjectAttribute") ||
                fullName.EndsWith("Texture2DAttribute") ||
                fullName.Contains("Texture2DAttribute"))
            {
                attributes.Add(attribute);
            }
        }

        return attributes;
    }

    private static bool IsTexture2DAttribute(AttributeData attribute)
    {
        var attributeClass = attribute.AttributeClass;
        if (attributeClass == null)
            return false;

        var fullName = attributeClass.ToDisplayString();
        return fullName.EndsWith("Texture2DAttribute") ||
               fullName.Contains("Texture2DAttribute");
    }

    private static string GetPropertyName(AttributeData attribute)
    {
        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key == "PropertyName" && namedArgument.Value.Value is string propertyName)
            {
                return propertyName;
            }
        }

        if (GetContentPath(attribute) is string contentPath)
        {
            return GeneratePropertyNameFromPath(contentPath);
        }

        return string.Empty;
    }

    private static string GeneratePropertyNameFromPath(string contentPath)
    {
        var withoutExtension = System.IO.Path.GetFileNameWithoutExtension(contentPath);
        if (string.IsNullOrEmpty(withoutExtension))
            return "UnknownContent";

        var propertyName = withoutExtension
            .Replace('/', '_')
            .Replace('\\', '_')
            .Replace('.', '_')
            .Replace(' ', '_');

        if (propertyName.Length > 0 && !char.IsLetter(propertyName[0]))
        {
            propertyName = "Content_" + propertyName;
        }

        if (!SyntaxFacts.IsValidIdentifier(propertyName))
        {
            propertyName = "Content_" + propertyName;
        }

        return propertyName;
    }

    private static string? GetContentPath(AttributeData attribute)
    {
        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key == "ContentPath" && namedArgument.Value.Value is string contentPath)
            {
                return contentPath;
            }
        }

        return null;
    }

    private static ITypeSymbol? GetContentType(Compilation compilation, AttributeData attribute)
    {
        var attributeClass = attribute.AttributeClass;
        if (attributeClass == null)
            return null;

        if (IsTexture2DAttribute(attribute))
        {
            var texture2DType = compilation.GetTypeByMetadataName("Microsoft.Xna.Framework.Graphics.Texture2D");
            if (texture2DType != null)
                return texture2DType;
        }

        if (attributeClass.IsGenericType &&
            attributeClass.TypeArguments.Length > 0)
        {
            return attributeClass.TypeArguments[0];
        }

        return null;
    }

    private static string GetFullTypeName(ITypeSymbol typeSymbol)
    {
        return $"global::{typeSymbol.ToDisplayString()}";
    }
}