using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.CommandLine.Tests
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

            var sourceDirectory = FindSourceDirectory();
            var cliAssembly = FindCliAssembly(sourceDirectory);
            var sampleDirectory = Path.Combine(sourceDirectory, "Samples", sampleName);

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

        private static string FindSourceDirectory([CallerFilePath] string testFilePath = "")
        {
            // Navigate up from src/Tools/Tests/CommandLine/ to src/
            var sourceDirectory = Directory.GetParent(Path.GetDirectoryName(testFilePath)!)?.Parent?.Parent?.FullName;
            if (sourceDirectory is not null && IsSourceDirectory(sourceDirectory))
                return sourceDirectory;

            throw new DirectoryNotFoundException("The source directory could not be found.");
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
                foreach (var configuration in new[] { "Debug", "Release" })
                {
                    var artifactAssembly = Path.Combine(dotvvmRoot, "artifacts", "bin", "DotVVM.CommandLine", configuration, "net8.0", "dotnet-dotvvm.dll");
                    if (File.Exists(artifactAssembly))
                        return artifactAssembly;
                }
            }

            foreach (var configuration in new[] { "Debug", "Release" })
            {
                var localAssembly = Path.Combine(sourceDirectory, "Tools", "CommandLine", "bin", configuration, "net8.0", "dotnet-dotvvm.dll");
                if (File.Exists(localAssembly))
                    return localAssembly;
            }

            return Path.Combine(sourceDirectory, "Tools", "CommandLine", "bin", "Debug", "net8.0", "dotnet-dotvvm.dll");
        }
    }
}
