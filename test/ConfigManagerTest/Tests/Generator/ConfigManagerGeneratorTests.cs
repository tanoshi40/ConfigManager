using ConfigManager.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ConfigManagerTest.Tests.Generator;


[UsesVerify]
public class ConfigManagerGeneratorTests
{
    static GeneratorDriver GeneratorDriver()
    {
        CSharpCompilation compilation = CSharpCompilation.Create(nameof(ConfigPropertyChangeGenerator));
        ConfigPropertyChangeGenerator generator = new();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        return driver.RunGenerators(compilation);
    }

    [Fact]
    public Task TestDriver()
    {
        GeneratorDriver driver = GeneratorDriver();
        return Verify(driver);
    }
}
