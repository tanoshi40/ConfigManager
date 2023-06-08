using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using ConfigManager.Generator.CodeSyntax;
using ConfigManager.Generator.PredicateUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConfigManager.Generator;

[Generator(LanguageNames.CSharp)]
internal sealed class ConfigPropertyChangeGenerator : IIncrementalGenerator
{
    internal const string GenName = nameof(ConfigPropertyChangeGenerator);
    internal const string AttributeNamespace = "ConfigManager.Attributes";

    internal static readonly string Version;
    internal static readonly AttributeSyntax CodeGenAttribute;
    internal static readonly CodeSyntaxDefinitions.Attribute[] ConstantAttributes;

    static ConfigPropertyChangeGenerator()
    {
        Version = typeof(ConfigPropertyChangeGenerator).GetAssemblyVersion();
        CodeGenAttribute = GeneratorHelper.BuildGeneratorAttribute(GenName, Version);

        ConstantAttributes = new CodeSyntaxDefinitions.Attribute[]
        {
            new("Config", AttributeTargets.Class), new("ConfigIgnore", AttributeTargets.Field)
        };
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AddStaticSources);

        context.SyntaxProvider.CreateSyntaxProvider(SyntacticPredicate, SemanticTransform);
    }

    private static bool SyntacticPredicate(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax
        {
            AttributeLists.Count: > 0
        } candidate
        && candidate.Modifiers.Any(SyntaxKind.PartialKeyword)
        && !candidate.Modifiers.Any(SyntaxKind.StaticKeyword);

    private static (INamedTypeSymbol type, INamedTypeSymbol typeofSymbol)? SemanticTransform(
        GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        ClassDeclarationSyntax candidate = (context.Node as ClassDeclarationSyntax)!;
        ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(candidate, cancellationToken);

        if (symbol is not INamedTypeSymbol type)
        {
            return null;
        }

        const string configInterfaceName = "";
        const string configAttributeName = "";

        Compilation compilation = context.SemanticModel.Compilation;
        // get config interface type
        if (!SemanticTransformHelper.TryGetTypeByName(compilation, configInterfaceName,
                out INamedTypeSymbol? configInterface))
        {
            return null;
        }

        // get config attribute type
        if (!SemanticTransformHelper.TryGetTypeByName(compilation, configAttributeName,
                out INamedTypeSymbol? configAttribute))
        {
            return null;
        }

        // type does not have config interface 
        if (SemanticTransformHelper.HasInterface(configInterface, type))
        {
            return null;
        }

        // type does have config attribute 
        if (!SemanticTransformHelper.TryGetAttributeTypeOfInfo(candidate, configAttribute, context.SemanticModel,
                out INamedTypeSymbol? attribute))
        {
            return (type, attribute!);
        }

        return null;
    }


    private static void AddStaticSources(IncrementalGeneratorPostInitializationContext context)
    {
        foreach (CodeSyntaxDefinitions.Attribute attribute in ConstantAttributes)
        {
            context.AddSource(attribute.FileName, attribute.AsCodeContext(CodeGenAttribute, AttributeNamespace));
        }
    }
}
