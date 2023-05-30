using Microsoft.CodeAnalysis;

namespace ConfigManager.Generator;

public class ConfigPropertyChangeGenerator : IIncrementalGenerator
{
    private readonly string _version;
    

    public ConfigPropertyChangeGenerator() => _version = GetType().Assembly.GetName().Version?.ToString() ?? "n/a";


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
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
