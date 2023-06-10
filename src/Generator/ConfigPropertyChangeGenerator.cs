using System;
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
    internal const string InterfacesNamespace = "ConfigManager.Interfaces";

    internal static readonly string Version;
    internal static readonly AttributeSyntax CodeGenAttribute;
    internal static readonly CodeSyntaxDefinitions.Type[] UnconditionalClassesToAdd;

    internal static readonly CodeSyntaxDefinitions.Attribute ConfigAttribute;
    internal static readonly CodeSyntaxDefinitions.Attribute ConfigIgnoreAttribute;

    internal static readonly CodeSyntaxDefinitions.Interface ConfigInterface;

    static ConfigPropertyChangeGenerator()
    {
        Version = typeof(ConfigPropertyChangeGenerator).GetAssemblyVersion();
        CodeGenAttribute = GeneratorHelper.BuildGeneratorAttribute(GenName, Version);

        ConfigAttribute = new("Config", AttributeTargets.Class);
        ConfigIgnoreAttribute = new("ConfigIgnore", AttributeTargets.Field);

        ConfigInterface = new(
            "IConfig",
            methods: new CodeSyntaxDefinitions.Method[]
            {
                new("Save", "void"), new("Load", "IConfig", otherModifiers: new[] {Modifier.Static})
            });

        UnconditionalClassesToAdd =
            new CodeSyntaxDefinitions.Type[] {ConfigAttribute, ConfigIgnoreAttribute, ConfigInterface};
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

        string configInterfaceName = $"{AttributeNamespace}.{ConfigInterface.Name}";
        string configAttributeName = $"{InterfacesNamespace}.{ConfigAttribute.Name}";

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
        foreach (CodeSyntaxDefinitions.Type type in UnconditionalClassesToAdd)
        {
            context.AddSource(type.FileName, type.AsCodeContext(CodeGenAttribute, AttributeNamespace));
        }
    }
}
