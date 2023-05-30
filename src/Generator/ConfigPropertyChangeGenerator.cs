using System;
using ConfigManager.Generator.CodeSyntaxDeclarations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static ConfigManager.Generator.CodeSyntaxDeclarations.CodeSyntaxDefinitions;

namespace ConfigManager.Generator;

public class ConfigPropertyChangeGenerator : IIncrementalGenerator
{
    internal const string GenName = nameof(ConfigPropertyChangeGenerator);
    internal const string AttributeNamespace = "ConfigManager.Attributes";

    internal static readonly string Version;
    internal static readonly string GenAttributeStr;
    internal static readonly AttributeSyntax CodeGenAttribute;
    internal static readonly AttributeCode[] ConstantAttributes;

    static ConfigPropertyChangeGenerator()
    {
        Version = typeof(ConfigPropertyChangeGenerator).GetAssemblyVersion();
        
        CodeGenAttribute = SyntaxBuilder.BuildGeneratorAttribute(GenName, Version);
        GenAttributeStr = $"[{CodeGenAttribute.ToString()}]";
        
        ConstantAttributes = new AttributeCode[]
        {
            new DefinedAttributeCode("Config", AttributeNamespace, GenAttributeStr,
                Array.Empty<Property>(), new[] {AttributeTargets.Class}),
            // new DefinedAttributeCode("ConfigInclude", AttributeNamespace, _genAttribute,
            //     Array.Empty<PropertyCode>(), new[] {AttributeTargets.Field, AttributeTargets.Method, AttributeTargets.Property}),
            new DefinedAttributeCode("ConfigExclude", AttributeNamespace, GenAttributeStr,
                Array.Empty<Property>(), new[] {AttributeTargets.Field}),
            new DefinedAttributeCode("TMP", AttributeNamespace, GenAttributeStr,
                new[]
                {
                    new Property("Prop1", "string").WithAlternativeName("prop1"),
                    new Property("Prop2", "string").WithAlternativeName("prop2"),
                    new Property("Prop3", "int").WithAlternativeName("prop3")
                }, Array.Empty<AttributeTargets>())
        };
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialization);
    }

    private void PostInitialization(IncrementalGeneratorPostInitializationContext context)
    {
        foreach (AttributeCode attributeCode in ConstantAttributes)
        {
            context.AddSource(attributeCode.FileName, attributeCode.Code);
        }
    }
}
