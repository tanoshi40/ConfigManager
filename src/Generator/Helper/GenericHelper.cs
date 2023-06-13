using System;
using System.Linq;

namespace ConfigManager.Generator.Helper;

internal static partial class GeneratorHelper
{
    
    // TODO: move to Malwis once it is accessible via nuget
    internal static T[] JoinArray<T>(this T[] input, T separator)
    {
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

    // TODO: move to Malwis once it is accessible via nuget
    internal static T[] SplitFlagEnum<T>(this T flaggedEnum) where T : Enum, IConvertible =>
        Enum.GetValues(typeof(T)).Cast<T>().Where(e => flaggedEnum.HasFlag(e)).ToArray();
}
