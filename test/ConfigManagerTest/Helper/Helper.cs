using ConfigManager.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ConfigManagerTest.Helper;

public static class TestHelper
{
    public static GeneratorDriver GenerateDriver(params string[] sources)
    {
        SyntaxTree[] syntaxTrees = sources.Select(src => CSharpSyntaxTree.ParseText(src)).ToArray();

        CSharpCompilation compilation = CSharpCompilation.Create(nameof(ConfigPropertyChangeGenerator), syntaxTrees);
        ConfigPropertyChangeGenerator generator = new();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        return driver.RunGenerators(compilation);
    }
}
