using System;
using ConfigManager.Generator.CodeStructures;
using Microsoft.CodeAnalysis;

namespace ConfigManager.Generator;

public class ConfigPropertyChangeGenerator : IIncrementalGenerator
{
    private const string GenName = nameof(ConfigPropertyChangeGenerator);
    private const string AttributeNamespace = "ConfigManager.Attributes";

    private readonly string _version;
    private readonly string _genAttribute;
    private readonly AttributeCode[] _constantAttributes;

    public ConfigPropertyChangeGenerator()
    {
        _version = GetType().Assembly.GetName().Version?.ToString() ?? "n/a";
        _genAttribute = GeneratorHelper.GetGeneratedAttribute(GenName, _version);
        _constantAttributes = new AttributeCode[]
        {
            new DefinedAttributeCode("Config", AttributeNamespace, _genAttribute, 
                Array.Empty<PropertyCode>(), new[] {AttributeTargets.Class}),
            new DefinedAttributeCode("ConfigInclude", AttributeNamespace, _genAttribute,
                Array.Empty<PropertyCode>(), new[] {AttributeTargets.Field, AttributeTargets.Method}),
            new DefinedAttributeCode("ConfigExclude", AttributeNamespace, _genAttribute,
                Array.Empty<PropertyCode>(), new[] {AttributeTargets.Field})
        };
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialization);
        
    }

    private void PostInitialization(IncrementalGeneratorPostInitializationContext context)
    {
        foreach (AttributeCode attributeCode in _constantAttributes)
        {
            context.AddSource(attributeCode.FileName, attributeCode.Code);
        }
    }
}

/*
[AttributeUsage(AttributeTargets.Class)]
public class ConfigAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)] // | AttributeTargets.Field | AttributeTargets.Method )]
public class ConfigIncludeAttribute : Attribute
{
}
*/
