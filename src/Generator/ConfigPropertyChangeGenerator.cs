using System;
using System.Collections.Generic;
using System.Threading;
using ConfigManager.Generator.CodeSyntax;
using ConfigManager.Generator.Helper;
using ConfigManager.Generator.PredicateUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ConfigManager.Generator;

[Generator(LanguageNames.CSharp)]
internal sealed class ConfigPropertyChangeGenerator : IIncrementalGenerator, IGenerator
{
    internal static readonly string GenVersion;
    internal const string GenName = nameof(ConfigPropertyChangeGenerator);

    private const string BaseNamespace = "ConfigManager";
    private const string AttributeNamespace = $"{BaseNamespace}.Attributes";
    private const string InterfacesNamespace = $"{BaseNamespace}.Interfaces";

    private static readonly string ConfigInterfaceName;
    private static readonly string ConfigAttributeName;
    private static readonly string ConfigIgnoreAttributeName;

    private static readonly AttributeSyntax CodeGenAttribute;
    private static readonly CodeSyntaxDefinitions.Type[] UnconditionalClassesToAdd;

    private static readonly CodeSyntaxDefinitions.Attribute ConfigAttribute;
    private static readonly CodeSyntaxDefinitions.Attribute ConfigIgnoreAttribute;

    private static readonly CodeSyntaxDefinitions.Interface ConfigInterface;

    public string Name => GenName;
    public string Version => GenVersion;

    static ConfigPropertyChangeGenerator()
    {
        GenVersion = typeof(ConfigPropertyChangeGenerator).GetAssemblyVersion();
        CodeGenAttribute = GeneratorHelper.BuildGeneratorAttribute(GenName, GenVersion);

        ConfigAttribute = new("Config", AttributeTargets.Class);
        ConfigIgnoreAttribute = new("ConfigIgnore", AttributeTargets.Field);

        ConfigInterface = CreateConfigInterface();

        ConfigInterfaceName = $"{InterfacesNamespace}.{ConfigInterface.Name}";
        ConfigAttributeName = $"{AttributeNamespace}.{ConfigAttribute.Name}";
        ConfigIgnoreAttributeName = $"{AttributeNamespace}.{ConfigIgnoreAttribute.Name}";
        
        UnconditionalClassesToAdd =
            new CodeSyntaxDefinitions.Type[] {ConfigAttribute, ConfigIgnoreAttribute, ConfigInterface};
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AddStaticSources);

        // class, partial, no static, 1+ attributes, has ConfAttr, has no ConfIntf
        var pipeline = context.SyntaxProvider
            .CreateSyntaxProvider(SyntacticPredicate, SemanticTransform)

            .Where(static x => x is not null)
            .Select(static (x, _) => x!)
            .Select(SelectFields); // Select fields

        context.RegisterSourceOutput(pipeline, Exec);
    }

    private void Exec(SourceProductionContext arg1, IEnumerable<IFieldSymbol> fieldSymbols)
    {
        // TODO: generate code here
    }

    private static IEnumerable<IFieldSymbol> SelectFields(INamedTypeSymbol configClass,
        CancellationToken ct)
    {
        List<IFieldSymbol> fields = new();
        
        foreach (ISymbol? member in configClass.GetMembers())
        {
            ct.ThrowIfCancellationRequested();
            if (member is IFieldSymbol
                {
                    IsImplicitlyDeclared: false,
                    Kind: SymbolKind.Field
                } field && !field.HasAttribute(ConfigIgnoreAttributeName))
            {
                fields.Add(field);
            }
        }

        return fields;
    }

    private static bool SyntacticPredicate(SyntaxNode node, CancellationToken cancellationToken) =>
        node is ClassDeclarationSyntax
        {
            AttributeLists.Count: > 0
        } candidate
        && candidate.Modifiers.Any(SyntaxKind.PartialKeyword)
        && !candidate.Modifiers.Any(SyntaxKind.StaticKeyword);

    private static INamedTypeSymbol? SemanticTransform(
        GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        ClassDeclarationSyntax candidate = (context.Node as ClassDeclarationSyntax)!;
        ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(candidate, cancellationToken);

        if (symbol is not INamedTypeSymbol type)
        {
            return null;
        }


        // type does not have config interface 
        if (type.HasInterface(ConfigInterfaceName))
        {
            return null;
        }

        // type does have config attribute 
        return !candidate.HasAttribute(ConfigAttributeName, context.SemanticModel, cancellationToken)
            ? type
            : null;
    }


    private static void AddStaticSources(IncrementalGeneratorPostInitializationContext context)
    {
        foreach (CodeSyntaxDefinitions.Type type in UnconditionalClassesToAdd)
        {
            context.AddSource(type.FileName, type.AsCodeContext(CodeGenAttribute, AttributeNamespace));
        }
    }

    private static CodeSyntaxDefinitions.Interface CreateConfigInterface()
    {
        CodeSyntaxDefinitions.Method saveMethod = new("Save", "void");
        CodeSyntaxDefinitions.Method loadMethod = new("Load", "IConfig", otherModifiers: new[] {Modifier.Static},
            body: Block(SingletonList<StatementSyntax>(
                ThrowStatement(
                    ObjectCreationExpression(
                            IdentifierName("NotImplementedException"))
                        .WithArgumentList(ArgumentList()))))
        );

        return new(
            "IConfig",
            methods: new[] {saveMethod, loadMethod});
    }
}
