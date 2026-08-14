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
            var result = await RunLintCommand(cliAssembly, sampleDirectory);

            try
            {
                Assert.AreNotEqual(0, result.ExitCode);
                StringAssert.Contains(
                    result.Output,
                    "Views/Errors/MissingViewModel.dothtml(1,0): error: The @viewModel directive is missing in the page 'Views/Errors/MissingViewModel.dothtml'!");
            }
            catch
            {
                TestContext.WriteLine(result.Output);
                throw;
            }
        }

        [DataTestMethod]
        [DataRow("new-app-net10.0-correct", false, "")]
        [DataRow("new-app-net10.0-errors", true, "Pages/Default/default.dothtml(1,0): error: Could not resolve type")]
        [DataRow("new-app-net472-correct", false, "")]
        [DataRow("new-app-net472-errors", true, "Views/default.dothtml(1,0): error: Could not resolve type")]
        [DataRow("new-app-net8.0-correct", false, "")]
        [DataRow("new-app-net8.0-errors", true, "Pages/Default/default.dothtml(1,0): error: Could not resolve type")]
        public async Task LintCommand_ReportsExpectedDiagnosticsForTestProjects(string projectName, bool hasErrors, string expectedDiagnostic)
        {
            var sourceDirectory = FindSourceDirectory();
            var cliAssembly = FindCliAssembly(sourceDirectory);
            var projectDirectory = Path.Combine(sourceDirectory, "Tools", "Tests", "CommandLine-TestProjects", projectName);
            var result = await RunLintCommand(cliAssembly, projectDirectory);

            try
            {
                Assert.AreEqual(hasErrors ? 1 : 0, result.ExitCode);
                if (hasErrors)
                {
                    StringAssert.Contains(result.Output, expectedDiagnostic);
                }
            }
            catch
            {
                TestContext.WriteLine(result.Output);
                throw;
            }
        }

        private static async Task<(int ExitCode, string Output)> RunLintCommand(string cliAssembly, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo("dotnet")
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = workingDirectory,
                ArgumentList = { "exec", cliAssembly, "lint", "--no-color", "--verbose-build-output" }
            };
            startInfo.EnvironmentVariables.Remove("DOTVVM_ROOT");

            using var process = Process.Start(startInfo)!;
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            var processExit = process.WaitForExitAsync();
            await Task.WhenAll(standardOutput, standardError, processExit);

            return (process.ExitCode, await standardOutput + await standardError);
        }

        private static string FindSourceDirectory([CallerFilePath] string testFilePath = "")
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTVVM_ROOT")))
            {
                var srcDirectory = Path.Combine(Environment.GetEnvironmentVariable("DOTVVM_ROOT")!, "src");
                if (IsSourceDirectory(srcDirectory))
                    return srcDirectory;
            }

            var sourceDirectory = FindSourceDirectoryFromParentChain(Path.GetDirectoryName(testFilePath));
            if (sourceDirectory is not null)
                return sourceDirectory;

            throw new DirectoryNotFoundException("The source directory could not be found.");
        }

        private static string? FindSourceDirectoryFromParentChain(string? startDirectory)
        {
            for (var directory = startDirectory; directory is not null; directory = Directory.GetParent(directory)?.FullName)
            {
                if (IsSourceDirectory(directory))
                    return directory;
            }

            return null;
        }

        private static bool IsSourceDirectory(string directory) =>
            File.Exists(Path.Combine(directory, "Tools", "CommandLine", "DotVVM.CommandLine.csproj")) &&
            Directory.Exists(Path.Combine(directory, "Samples"));

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
