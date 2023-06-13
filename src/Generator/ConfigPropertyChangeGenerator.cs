using System;
using System.Threading;
using ConfigManager.Generator.CodeSyntax;
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

        string configInterfaceName = $"{InterfacesNamespace}.{ConfigInterface.Name}";
        string configAttributeName = $"{AttributeNamespace}.{ConfigAttribute.Name}";

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


    private void AddStaticSources(IncrementalGeneratorPostInitializationContext context)
    {
        this.DebugLine("adding static sources");
        foreach (CodeSyntaxDefinitions.Type type in UnconditionalClassesToAdd)
        {
            this.DebugLine("adding {}", type.FileName);
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
            methods: new[]
            {
                saveMethod, loadMethod
            });
    }
}
