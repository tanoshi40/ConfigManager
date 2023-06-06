namespace ConfigManagerTest;

#if DEBUG
public class Debug
{
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
}
#endif
