namespace ConfigManager.Generator.CodeStructures;

internal record struct PropertyCode(string Name, string LoweredName, string FullyQualifiedType)
{
    internal string GetPropertyDeclaration(string accessModifier = "public", string accessors = "get; set;") =>
        $"{accessModifier} {FullyQualifiedType} {Name} {accessors}";
}
