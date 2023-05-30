using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

namespace ConfigManagerTest.TestHelper;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    public static DiagnosticResult Diagnostic()
        => new DiagnosticResult();

    public static DiagnosticResult Diagnostic(string id, DiagnosticSeverity severity)
        => new DiagnosticResult(id, severity);

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => new DiagnosticResult(descriptor);

    public class VerifyBuilder
    {
        public string Source { get; set; }
        public List<DiagnosticResult> Diagnostics { get; set; } = new();
        public List<(string filename, string content)> GeneratedSources { get; set; } = new();

        internal VerifyBuilder(string source)
        {
            Source = source;
        }

        public VerifyBuilder WithDiagnostics(params DiagnosticResult[] diagnostics)
        {
            Diagnostics.AddRange(diagnostics);
            return this;
        }

        public VerifyBuilder WithDiagnostic(DiagnosticResult diagnostic)
        {
            Diagnostics.Add(diagnostic);
            return this;
        }

        public VerifyBuilder WithGeneratedSource(string filename, string content)
        {
            GeneratedSources.Add((filename, content));
            return this;
        }

        public VerifyBuilder WithGeneratedSources(params (string filename, string content)[] generatedSources)
        {
            GeneratedSources.AddRange(generatedSources);
            return this;
        }

        public async Task Verify()
        {
            Test test = new()
            {
                TestState =
                {
                    Sources = { Source },
                }
            };

            foreach ((string filename, string content) in GeneratedSources)
            {
                test.TestState.GeneratedSources.Add((typeof(TSourceGenerator), filename, SourceText.From(content, Encoding.UTF8)));
            }

            test.ExpectedDiagnostics.AddRange(Diagnostics);

            await test.RunAsync(CancellationToken.None);
        }
    }

    public static VerifyBuilder Verifier(string source) => new(source);
    
    private class Test : CSharpSourceGeneratorTest<EmptySourceGeneratorProvider, XUnitVerifier>
    {
        protected override CompilationOptions CreateCompilationOptions()
        {
            CompilationOptions compilationOptions = base.CreateCompilationOptions();
            return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

        protected override IEnumerable<ISourceGenerator> GetSourceGenerators()
        {
            return new[] {new TSourceGenerator().AsSourceGenerator()};
        }

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = {"/warnaserror:nullable"};
            CSharpCommandLineArguments commandLineArguments = CSharpCommandLineParser.Default.Parse(args,
                baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
            ImmutableDictionary<string, ReportDiagnostic> nullableWarnings =
                commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

            return nullableWarnings;
        }

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions) base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
        }
    }
}
