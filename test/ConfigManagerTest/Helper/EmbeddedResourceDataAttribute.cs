using System.Reflection;
using Xunit.Sdk;

namespace ConfigManagerTest.Helper;

public sealed class EmbeddedResourceDataAttribute : DataAttribute
{
    private static readonly Dictionary<string, string> TestResources = LoadTestSources();

    private static Dictionary<string, string> LoadTestSources()
    {
        Dictionary<string, string> sources = new();
        Assembly assembly = Assembly.GetExecutingAssembly();

        foreach (string file in Directory.GetFiles("./Resources"))
        {
            FileInfo fileInfo = new(file);
            using FileStream stream = fileInfo.OpenRead();
            using StreamReader reader = new(stream);
            sources[fileInfo.Name] = reader.ReadToEnd();
        }

        return sources;
    }


    private readonly string[] _args;

    public EmbeddedResourceDataAttribute(params string[] args) => _args = args;


    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        object[] result = new object[_args.Length];
        for (int index = 0; index < _args.Length; index++)
        {
            result[index] = ReadManifestData(_args[index]);
        }

        return new[] {result};
    }

    private static string ReadManifestData(string fileName)
    {
        if (TestResources.TryGetValue(fileName, out string? data))
        {
            return data;
        }
        
        throw new InvalidOperationException($"Could not find resource. {fileName}");
    }
}
