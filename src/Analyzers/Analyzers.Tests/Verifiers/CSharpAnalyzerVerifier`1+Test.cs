#nullable disable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace DotVVM.Analyzers.Tests
{
    public static partial class CSharpAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public class Test : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
        {
            public Test()
            {
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                    compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                    // Add DotVVM.Framework as a project reference
                    var locationFramework = System.Reflection.Assembly.GetAssembly(typeof(Framework.Controls.GridView)).Location;
                    solution = solution.AddMetadataReference(projectId, MetadataReference.CreateFromFile(locationFramework));
                    // Add DotVVM.Core as a project reference
                    var locationCore = System.Reflection.Assembly.GetAssembly(typeof(Framework.ViewModel.BindAttribute)).Location;
                    solution = solution.AddMetadataReference(projectId, MetadataReference.CreateFromFile(locationCore));
                    // Add Newtonsoft.Json as a reference
                    var locationNewtonsoft = System.Reflection.Assembly.GetAssembly(typeof(Newtonsoft.Json.JsonConvert)).Location;
                    solution = solution.AddMetadataReference(projectId, MetadataReference.CreateFromFile(locationNewtonsoft));

                    return solution;
                });
            }
        }
    }
}
