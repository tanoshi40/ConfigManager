namespace ConfigManager.Generator;

internal record struct PropertyCode(string CamelCaseName, string PascalCaseName, string FullyQualifiedType)
{
    internal string GetPropertyDeclaration(string accessModifier = "public", string accessors = "get; set;") =>
        $"{accessModifier} {FullyQualifiedType} {PascalCaseName} {accessors}";
}
