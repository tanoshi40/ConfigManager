using System;

namespace ConfigManager.Generator;

public static class GeneratorHelper
{
    internal const string NewLine = @"
";

    internal static string GetGeneratedAttribute(string genName, string genVersion) =>
        $@"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(tool:""{genName}"", version:""{genVersion}"")]";

    internal static string GetAssemblyVersion(this Type type) => type.Assembly.GetName().Version?.ToString() ?? "n/a";


    internal static string GetFullQualifiedName(this AttributeTargets target) =>
        $"global::System.AttributeTargets.{target.ToString()}";

    internal static string ToLowerString(this bool @bool) => @bool.ToString().ToLower();

    public static T[] JoinArray<T>(this T[] input, T separator)
    {
        // TODO: move to Malwis once it is accessible via nuget
        if (input.Length < 2) { return input; }

        T[] joined = new T[input.Length + input.Length - 1];

        for (int srcI = 0, targetI = 0; srcI < input.Length; srcI++, targetI += 2)
        {
            joined[targetI] = input[srcI];
            if (targetI + 1 < joined.Length)
            {
                joined[targetI + 1] = separator;
            }
        }

        return joined;
    }
}
