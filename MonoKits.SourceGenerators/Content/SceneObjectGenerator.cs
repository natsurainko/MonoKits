using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MonoKits.SourceGenerators.Content;

[Generator]
public class SceneObjectSourceGenerator : IIncrementalGenerator
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
               classDeclaration.BaseList?.Types.Count > 0;
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        foreach (var baseType in classDeclaration.BaseList!.Types)
        {
            if (baseType.Type is GenericNameSyntax genericName &&
                genericName.Identifier.ValueText == "Scene3D")
            {
                return classDeclaration;
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
                    context.AddSource($"{classSymbol.Name}.SceneObjects.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SOG001",
                        "Scene object code generation error",
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
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;

        var loadObjectsStatements = new StringBuilder();
        var loadPropertiesStatements = new StringBuilder();
        var loadObjectsIntoSceneStatements = new StringBuilder();
        var loadObjectsIntoPhysicsSystemStatements = new StringBuilder();

        foreach (var property in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var attributeDatas = property.GetAttributes();
            if (attributeDatas.Length == 0) continue;

            var sceneObjectAttr = GetAttribute(attributeDatas, "SceneObject");
            var positionAttr = GetAttribute(attributeDatas, "SceneObjectPosition");
            var rotationAttr = GetAttribute(attributeDatas, "SceneObjectRotation");

            var physicsObjectAttr = GetAttribute(attributeDatas, "PhysicsObject");

            var staticModelAttr = GetAttribute(attributeDatas, "StaticModel");

            var propertyName = property.Name;

            if (staticModelAttr != null)
            {
                var modelPropertyName = staticModelAttr.ConstructorArguments[0].Value?.ToString();
                if (string.IsNullOrEmpty(modelPropertyName)) continue;

                loadObjectsStatements.AppendLine($"        {propertyName} = new global::MonoKits.Spatial3D.Objects.Physics.StaticModelObject3D(ScopedContent.{modelPropertyName});");
            }

            if (positionAttr != null)
            {
                var x = positionAttr.ConstructorArguments[0].Value;
                var y = positionAttr.ConstructorArguments[1].Value;
                var z = positionAttr.ConstructorArguments[2].Value;
                loadPropertiesStatements.AppendLine($"        {propertyName}!.Position = new global::Microsoft.Xna.Framework.Vector3({x}f, {y}f, {z}f);");
            }

            if (rotationAttr != null)
            {
                var x = rotationAttr.ConstructorArguments[0].Value;
                var y = rotationAttr.ConstructorArguments[1].Value;
                var z = rotationAttr.ConstructorArguments[2].Value;
                loadPropertiesStatements.AppendLine($"        {propertyName}!.Rotation = new global::Microsoft.Xna.Framework.Vector3({x}f, {y}f, {z}f);");
            }

            if (sceneObjectAttr != null)
            {
                loadObjectsIntoSceneStatements.AppendLine($"        sceneManager.AddObject({propertyName}!);");
            }

            if (physicsObjectAttr != null)
            {
                loadObjectsIntoPhysicsSystemStatements.AppendLine($"        physicsSystem.Add({propertyName}!);");
            }
        }

        var sourceCode = $$"""
            // <auto-generated/>
            #nullable enable
            
            using Microsoft.Xna.Framework;
            using MonoKits.Spatial3D;
            using MonoKits.Spatial3D.Physics;
            
            namespace {{namespaceName}};
            
            partial class {{className}}
            {
                protected override void LoadObjects()
                {
                    ScopedContent.Load();
            {{loadObjectsStatements}}
                }

                protected override void LoadProperties()
                {
            {{loadPropertiesStatements}}
                }
            
                protected override void LoadObjectsIntoScene(SceneManager sceneManager)
                {
                    sceneManager.ClearScene();
            {{loadObjectsIntoSceneStatements}}
                }

                protected override void LoadObjectsIntoPhysicsSystem(PhysicsSystem? physicsSystem)
                {
                    if (physicsSystem == null) return;

            {{loadObjectsIntoPhysicsSystemStatements}}
                }
            }
            """;

        return sourceCode;
    }

    private static AttributeData? GetAttribute(ImmutableArray<AttributeData> attributes, string name)
    {
        return attributes
            .FirstOrDefault(attr =>
                attr.AttributeClass != null && (
                attr.AttributeClass.ToDisplayString().EndsWith($"{name}Attribute") ||
                attr.AttributeClass.ToDisplayString().Contains($"{name}Attribute")));
    }
}