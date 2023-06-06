using System.Runtime.CompilerServices;

namespace ConfigManagerTest;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void InitTests() => VerifySourceGenerators.Initialize();
}
