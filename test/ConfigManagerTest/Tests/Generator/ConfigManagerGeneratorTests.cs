using ConfigManagerTest.Helper;
using Microsoft.CodeAnalysis;
using static ConfigManagerTest.Helper.TestHelper;

namespace ConfigManagerTest.Tests.Generator;

[UsesVerify]
public class ConfigManagerGeneratorTests
{


    [Theory]
    [EmbeddedResourceData("ConfigClass._cs")]
    public Task TestSimpleConfig(string data)
    {
        GeneratorDriver driver = GenerateDriver(data);
        return Verify(driver);
    }

    [Fact]
    public Task TestDriver()
    {
        GeneratorDriver driver = GenerateDriver();
        return Verify(driver);
    }
}
