using System.Diagnostics;
using ConfigManager.Generator;
using ConfigManager.Generator.CodeSyntaxDeclarations;
using ConfigManagerTest.TestHelper;

namespace ConfigManagerTest;

using VerifyCS = CSharpSourceGeneratorVerifier<ConfigPropertyChangeGenerator>;

public class ConfigManagerGeneratorTests
{
    // TODO: add snapshot tests or direct tests

    [Fact]
    public async Task Test_NoCandidates_AddAttributes()
    {
        // Arrange
        const string source =
            """
            #nullable enable
            namespace Test123;

            public class Test
            {
                public void Empty()
                {
                    
                }
            }
            """;

        Debug.WriteLine(string.Join("\n \n \n \n",
            ConfigPropertyChangeGenerator.ConstantAttributes.Select(atr => $"{atr.FileName}:\n{atr.Code}")));

        // Act / Assert
        await VerifyCS.Verifier(source)
            .WithGeneratedSources(ConfigPropertyChangeGenerator.ConstantAttributes
                .Select(atrCode => (atrCode.FileName, atrCode.Code)).ToArray())
            .Verify();
    }

#if DEBUG
    [Fact]
    public void DebugEntrypoint()
    {
        CodeSyntaxDefinitions.Field[] members =
        {
            new("field1", "string", Modifier.Public), new("field2", "string", Modifier.Public),
            new("field3", "string", Modifier.Public), new("field4", "string", Modifier.Private),
            new("field5", "string", Modifier.Private), new CodeSyntaxDefinitions.Property("Age", "int"),
            new CodeSyntaxDefinitions.Property("UserName", "string", hasSetter: true),
            new CodeSyntaxDefinitions.Property("Name", "string", Modifier.Private),
            new CodeSyntaxDefinitions.Property("Password", "string", Modifier.Private)
        };

        CodeSyntaxDefinitions.Clazz clazz = CodeSyntaxDefinitions.Clazz.CreateInstance("User", Modifier.Public,
            members: members, otherModifiers: new[] {Modifier.Sealed});


        string classCode = clazz.GetCode(ConfigPropertyChangeGenerator.CodeGenAttribute);
        
        Debug.WriteLine(classCode);
    }
#endif
}
