#if NET
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.CommandLine
{
    [TestClass]
    public class LintCommandTests
    {
        public TestContext TestContext { get; set; } = null!;

        [DataTestMethod]
        [DataRow("AspNetCoreLatest")]
        [DataRow("AspNetCore")]
        [DataRow("Owin")]
        public async Task LintCommand_ReportsDiagnosticsAndReturnsFailure(string sampleName)
        {
            if (sampleName == "Owin" && !OperatingSystem.IsWindows())
            {
                Assert.Inconclusive("OWIN sample is only supported on Windows.");
            }

            var repositoryRoot = FindRepositoryRoot();
            var cliAssembly = FindCliAssembly(repositoryRoot);
            var sampleDirectory = Path.Combine(repositoryRoot, "Samples", sampleName);

            var startInfo = new ProcessStartInfo("dotnet")
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = sampleDirectory,
                Arguments = $"exec {QuoteArgument(cliAssembly)} lint --no-color --verbose-build-output"
            };
            startInfo.EnvironmentVariables.Remove("DOTVVM_ROOT");

            using var process = Process.Start(startInfo)!;
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            var processExit = Task.Run(process.WaitForExit);
            await Task.WhenAll(standardOutput, standardError, processExit);
            var output = await standardOutput + await standardError;

            try
            {
                Assert.AreNotEqual(0, process.ExitCode);
                StringAssert.Contains(
                    output,
                    "Views/Errors/MissingViewModel.dothtml(1,0): error: The @viewModel directive is missing in the page 'Views/Errors/MissingViewModel.dothtml'!");
            }
            catch
            {
                TestContext.WriteLine(output);
                throw;
            }
        }

        private static string FindRepositoryRoot([CallerFilePath] string testFilePath = "")
        {
            var sourceDirectory = Directory.GetParent(Path.GetDirectoryName(testFilePath)!)?.Parent?.FullName;
            if (sourceDirectory is not null && IsSourceDirectory(sourceDirectory))
                return sourceDirectory;

            throw new DirectoryNotFoundException("The repository root could not be found.");
        }

        private static bool IsSourceDirectory(string directory) =>
            File.Exists(Path.Combine(directory, "Tools", "CommandLine", "DotVVM.CommandLine.csproj")) &&
            Directory.Exists(Path.Combine(directory, "Samples"));

        private static string QuoteArgument(string argument) => $"\"{argument.Replace("\"", "\\\"")}\"";

        private static string FindCliAssembly(string sourceDirectory)
        {
            var dotvvmRoot = Environment.GetEnvironmentVariable("DOTVVM_ROOT");
            if (dotvvmRoot is not null)
            {
                var artifactAssembly = Path.Combine(dotvvmRoot, "artifacts", "bin", "DotVVM.CommandLine", "Debug", "net8.0", "dotnet-dotvvm.dll");
                if (File.Exists(artifactAssembly))
                    return artifactAssembly;
            }

            return Path.Combine(sourceDirectory, "Tools", "CommandLine", "bin", "Debug", "net8.0", "dotnet-dotvvm.dll");
        }
    }
}
#endif
