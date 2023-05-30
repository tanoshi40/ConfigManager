namespace ConfigManager.Generator;

internal static class GeneratorHelper
{
    internal static readonly string NewLine = @"\n";
    
    internal static string GetGeneratedAttribute(string genName, string genVersion) =>
        $@"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(tool:""{genName}"", version:""{genVersion}"")]";

    internal static string G() => "";
}
