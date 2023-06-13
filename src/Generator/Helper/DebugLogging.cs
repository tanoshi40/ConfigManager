using System;
using System.Diagnostics;

namespace ConfigManager.Generator.Helper;

internal static partial class GeneratorHelper
{
    internal static void DebugLine(this IGenerator gen, object? msg)
    {
#if DEBUG
        DebugLine(gen, msg?.ToString());
#endif
    }

    internal static void DebugLine(this IGenerator gen, object? msg, object[] args)
    {
#if DEBUG
        DebugLine(gen, msg?.ToString(), args);
#endif
    }

    internal static void DebugLine(this IGenerator gen, string? msg)
    {
#if DEBUG
        DebugLine(gen, msg, Array.Empty<object>());
#endif
    }

    internal static void DebugLine(this IGenerator gen, string? format, params object[] args)
    {
#if DEBUG
        string message = format is null
            ? "null"
            : args.Length == 0
                ? format
                : string.Format(format, args);

        string prefix = $"[{gen.Name}:{gen.Version}]";
        Debug.WriteLine($"{prefix} {message}");
#endif
    }
}
