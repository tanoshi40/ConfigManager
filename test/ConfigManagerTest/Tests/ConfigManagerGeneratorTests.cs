using ConfigManager.Generator;
using ConfigManagerTest.TestHelper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ConfigManagerTest.Tests;

using VerifyCS = CSharpSourceGeneratorVerifier<ConfigPropertyChangeGenerator>;

[UsesVerify]
public class ConfigManagerGeneratorTests
{
    // TODO: add snapshot tests or direct tests

    static GeneratorDriver GeneratorDriver()
    {
        CSharpCompilation compilation = CSharpCompilation.Create("name");
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

#if DEBUG
    [Fact]
    public void DebugEntrypoint()
    {
        
        // CodeSyntaxDefinitions.Field[] members =
        // {
        //     new("field1", "string", Modifier.Public), new("field2", "string", Modifier.Public),
        //     new("field3", "string", Modifier.Public), new("field4", "string", Modifier.Private),
        //     new("field5", "string", Modifier.Private), new CodeSyntaxDefinitions.Property("Age", "int"),
        //     new CodeSyntaxDefinitions.Property("UserName", "string", hasSetter: true),
        //     new CodeSyntaxDefinitions.Property("Name", "string", Modifier.Private),
        //     new CodeSyntaxDefinitions.Property("Password", "string", Modifier.Private)
        // };
        //
        // CodeSyntaxDefinitions.Clazz clazz = CodeSyntaxDefinitions.Clazz.CreateInstance("User", Modifier.Public,
        //     members: members, otherModifiers: new[] {Modifier.Sealed});
        //
        //
        // string classCode = clazz.GetCode(ConfigPropertyChangeGenerator.CodeGenAttribute);
        //
        // Debug.WriteLine(classCode);
    }
#endif
}
