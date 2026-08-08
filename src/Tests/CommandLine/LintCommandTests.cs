#if NET
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.CommandLine
{
    [TestClass]
    public class LintCommandTests
    {
        [DataTestMethod]
        [DataRow("AspNetCoreLatest")]
        [DataRow("AspNetCore")]
        [DataRow("Owin")]
        public async Task LintCommand_ReportsDiagnosticsAndReturnsFailure(string sampleName)
        {
            var repositoryRoot = FindRepositoryRoot();
            var cliAssembly = Path.Combine(repositoryRoot, "Tools", "CommandLine", "bin", "Debug", "net8.0", "dotnet-dotvvm.dll");
            var sampleDirectory = Path.Combine(repositoryRoot, "Samples", sampleName);

            var startInfo = new ProcessStartInfo("dotnet")
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = sampleDirectory
            };
            startInfo.ArgumentList.Add("exec");
            startInfo.ArgumentList.Add(cliAssembly);
            startInfo.ArgumentList.Add("lint");
            startInfo.ArgumentList.Add("--no-color");
            startInfo.ArgumentList.Add("--no-build");

            using var process = Process.Start(startInfo)!;
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var output = await standardOutput + await standardError;

            Assert.AreNotEqual(0, process.ExitCode);
            StringAssert.Contains(output, ": error:");
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "DotVVM.sln")))
                    return directory.FullName;
                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("The repository root could not be found.");
        }
    }
}
#endif
