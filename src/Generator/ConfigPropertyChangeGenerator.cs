using System;
using ConfigManager.Generator.CodeSyntaxDeclarations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConfigManager.Generator;

public class ConfigPropertyChangeGenerator : IIncrementalGenerator
{
    internal const string GenName = nameof(ConfigPropertyChangeGenerator);
    internal const string AttributeNamespace = "ConfigManager.Attributes";

    internal static readonly string Version;
    internal static readonly string GenAttributeStr;
    internal static readonly AttributeSyntax CodeGenAttribute;
    internal static readonly CodeSyntaxDefinitions.Attribute[] ConstantAttributes;

    static ConfigPropertyChangeGenerator()
    {
        Version = typeof(ConfigPropertyChangeGenerator).GetAssemblyVersion();

        CodeGenAttribute = SyntaxBuilder.BuildGeneratorAttribute(GenName, Version);
        GenAttributeStr = $"[{CodeGenAttribute.ToString()}]";

        ConstantAttributes = new CodeSyntaxDefinitions.Attribute[]
        {
            new("Config", AttributeTargets.Class),
            new("ConfigIgnore", AttributeTargets.Field)
        };
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialization);
    }

    private void PostInitialization(IncrementalGeneratorPostInitializationContext context)
    {
        foreach (CodeSyntaxDefinitions.Attribute attribute in ConstantAttributes)
        {
            context.AddSource(attribute.FileName, attribute.AsCodeContext(CodeGenAttribute, AttributeNamespace));
        }
    }
}
