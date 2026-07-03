using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class CompilationStatusApiTests
    {
        [TestMethod]
        public async Task GetStatusResponse_Success_Returns200WithoutBody()
        {
            var service = new FakeViewCompilationService(compileResult: true, failedFiles: []);

            var result = await DotvvmCompilationStatusApi.GetStatusResponse(service);

            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNull(result.ResponseBody);
        }

        [TestMethod]
        public async Task GetStatusResponse_Failure_Returns500WithJsonBody()
        {
            var failedFiles = ImmutableArray.Create(new DotHtmlFileInfo("/Views/Failing.dothtml"));
            var service = new FakeViewCompilationService(compileResult: false, failedFiles);

            var result = await DotvvmCompilationStatusApi.GetStatusResponse(service);

            Assert.AreEqual(500, result.StatusCode);
            Assert.IsNotNull(result.ResponseBody);
            StringAssert.Contains(result.ResponseBody, "/Views/Failing.dothtml");
        }

        [TestMethod]
        [Timeout(600000)]
        public void CompilationStatusMode_AspNetCoreSample_WritesStatusToStdout()
        {
            var repositoryRoot = FindRepositoryRoot();
            var sampleProjectPath = Path.Combine(repositoryRoot, "src", "Samples", "AspNetCore", "DotVVM.Samples.BasicSamples.AspNetCore.csproj");

            var buildResult = RunProcess(
                repositoryRoot,
                "build",
                $"\"{sampleProjectPath}\" --nologo --verbosity minimal"
            );
            Assert.AreEqual(0, buildResult.ExitCode, $"Building the sample failed.{Environment.NewLine}STDOUT:{Environment.NewLine}{buildResult.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{buildResult.StandardError}");

            var runResult = RunProcess(
                repositoryRoot,
                "run",
                $"--no-build --project \"{sampleProjectPath}\" --no-launch-profile",
                ("DOTVVM_COMPILATION_STATUS", "1")
            );

            var outputLines = runResult.StandardOutput
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .ToArray();
            var statusLineIndex = Array.FindLastIndex(outputLines, line => line == "200" || line == "500");
            Assert.IsTrue(statusLineIndex >= 0, $"The one-shot mode did not print status code to stdout.{Environment.NewLine}STDOUT:{Environment.NewLine}{runResult.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{runResult.StandardError}");

            var statusCode = int.Parse(outputLines[statusLineIndex], CultureInfo.InvariantCulture);
            Assert.AreEqual(statusCode == 200 ? 0 : 1, runResult.ExitCode, $"Unexpected process exit code for one-shot mode.{Environment.NewLine}STDOUT:{Environment.NewLine}{runResult.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{runResult.StandardError}");

            if (statusCode == 500)
            {
                Assert.IsTrue(statusLineIndex + 1 < outputLines.Length, "Expected a JSON payload with failed files when status code is 500.");
                using var _ = JsonDocument.Parse(outputLines[statusLineIndex + 1]);
            }
        }

        private static string FindRepositoryRoot()
        {
            var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
            while (currentDirectory is not null)
            {
                if (File.Exists(Path.Combine(currentDirectory.FullName, "src", "DotVVM.sln")))
                {
                    return currentDirectory.FullName;
                }
                currentDirectory = currentDirectory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate repository root containing src/DotVVM.sln.");
        }

        private static ProcessResult RunProcess(
            string workingDirectory,
            string command,
            string arguments,
            params (string Name, string Value)[] environmentVariables
        )
        {
            using var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "dotnet",
                    WorkingDirectory = workingDirectory,
                    Arguments = $"{command} {arguments}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            foreach (var (name, value) in environmentVariables)
            {
                process.StartInfo.EnvironmentVariables[name] = value;
            }

            process.Start();
            var standardOutputTask = Task.Run(() => process.StandardOutput.ReadToEnd());
            var standardErrorTask = Task.Run(() => process.StandardError.ReadToEnd());
            if (!process.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds))
            {
                try
                {
                    process.Kill();
                }
                catch (InvalidOperationException)
                {
                }
                Assert.Fail($"Process timed out. Command: dotnet {command} {arguments}");
            }
            Task.WaitAll(standardOutputTask, standardErrorTask);

            return new ProcessResult(standardOutputTask.Result, standardErrorTask.Result, process.ExitCode);
        }

        private sealed class ProcessResult
        {
            public ProcessResult(string standardOutput, string standardError, int exitCode)
            {
                StandardOutput = standardOutput;
                StandardError = standardError;
                ExitCode = exitCode;
            }

            public string StandardOutput { get; }
            public string StandardError { get; }
            public int ExitCode { get; }
        }

        private sealed class FakeViewCompilationService : IDotvvmViewCompilationService
        {
            private readonly bool compileResult;
            private readonly ImmutableArray<DotHtmlFileInfo> failedFiles;

            public FakeViewCompilationService(bool compileResult, ImmutableArray<DotHtmlFileInfo> failedFiles)
            {
                this.compileResult = compileResult;
                this.failedFiles = failedFiles;
            }

            public ImmutableArray<DotHtmlFileInfo> GetFilesWithFailedCompilation() => failedFiles;
            public ImmutableArray<DotHtmlFileInfo> GetMasterPages() => [];
            public ImmutableArray<DotHtmlFileInfo> GetControls() => [];
            public ImmutableArray<DotHtmlFileInfo> GetRoutes() => [];
            public bool BuildView(DotHtmlFileInfo file, out DotHtmlFileInfo? masterPage)
            {
                masterPage = null;
                return true;
            }
            public Task<bool> CompileAll(bool buildInParallel = true, bool forceRecompile = false) => Task.FromResult(compileResult);
            public void RegisterCompiledView(string filePath, DotVVM.Framework.Compilation.ViewCompiler.ControlBuilderDescriptor? descriptor, Exception? exception) { }
        }
    }
}
